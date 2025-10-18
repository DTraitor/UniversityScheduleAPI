using System.Collections.Concurrent;
using BusinessLogic.Configuration;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.GroupLessons;

public class GroupScheduleReader : IScheduleReader<GroupLesson, GroupLessonModified>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGroupRepository  _groupRepository;
    private readonly IScheduleParser<GroupLesson> _groupLessonParser;
    private readonly IOptions<ScheduleParsingOptions> _options;
    private readonly ILogger<GroupScheduleReader> _logger;

    public GroupScheduleReader(
        IHttpClientFactory httpClientFactory,
        IGroupRepository groupRepository,
        IScheduleParser<GroupLesson> groupLessonParser,
        IOptions<ScheduleParsingOptions> options,
        ILogger<GroupScheduleReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _groupRepository = groupRepository;
        _groupLessonParser = groupLessonParser;
        _options = options;
        _logger = logger;
    }

    public async Task<(IEnumerable<GroupLessonModified>, ICollection<GroupLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);

        var html = await httpClient.GetStringAsync("/schedule/group/list", cancellationToken);

        var groupsList = new HtmlDocument();
        groupsList.LoadHtml(html);

        var groupsData = ReadGroupsList(groupsList);
        ConcurrentBag<GroupLesson> groupLessons = new ConcurrentBag<GroupLesson>();
        ConcurrentBag<GroupLessonModified> groupsModifications = new ConcurrentBag<GroupLessonModified>();

        var allGroups = await _groupRepository.GetAllAsync(cancellationToken);
        var groupsLookup = allGroups.ToDictionary(g => g.GroupName, g => g);

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(groupsData, options, async (groupData, ct) =>
        {
            groupData.Item1.SchedulePageHash = groupsLookup.GetValueOrDefault(groupData.Item1.GroupName, new Group()).SchedulePageHash;

            var lessons = await FetchGroupScheduleAsync(groupData.Item2, groupData.Item1, ct);
            if (lessons == null)
                return;

            foreach (var lesson in lessons)
            {
                groupLessons.Add(lesson);
            }

            groupsModifications.Add(new()
            {
                GroupId = groupData.Item1.Id,
            });
        });

        var newGroupNames = new HashSet<string>(groupsData.Select(g => g.Item1.GroupName));
        var removedGroups = allGroups.Where(c => !newGroupNames.Contains(c.GroupName)).ToList();

        var parsedGroups = groupsData
            .Select(x => x.Item1)
            .ToDictionary(x => x.GroupName, x => x);

        var parsedGroupsOriginalIds = groupsData
            .Select(x => x.Item1)
            .ToDictionary(x => x.Id, x => x);

        _groupRepository.RemoveRange(removedGroups);
        _groupRepository.AddRange(parsedGroups.Values
            .Where(x => !groupsLookup.ContainsKey(x.GroupName))
            .Select(x =>
        {
            x.Id = 0;
            return x;
        }));
        _groupRepository.UpdateRange(groupsLookup.Values
            .Where(x => !removedGroups.Contains(x))
            .Select(x =>
            {
                x.SchedulePageHash = parsedGroups[x.GroupName].SchedulePageHash;
                parsedGroups[x.GroupName].Id = x.Id;
                return x;
            }));

        await _groupRepository.SaveChangesAsync(cancellationToken);

        foreach (var groupLesson in groupLessons)
        {
            groupLesson.GroupId = parsedGroupsOriginalIds[groupLesson.GroupId].Id;
        }

        foreach (var groupLessonModified in groupsModifications)
        {
            groupLessonModified.GroupId = parsedGroupsOriginalIds[groupLessonModified.GroupId].Id;
        }

        return (groupsModifications, groupLessons.ToList());
    }

    private async Task<IEnumerable<GroupLesson>?> FetchGroupScheduleAsync(string href, Group group, CancellationToken stoppingToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);
        var scheduleString = await httpClient.GetStringAsync(href, stoppingToken);

        var scheduleDoc = new HtmlDocument();
        scheduleDoc.LoadHtml(scheduleString);

        if (!_groupLessonParser.HasHashChanged(scheduleDoc, group.SchedulePageHash, out var newHash))
            return null;
        group.SchedulePageHash = newHash;

        return _groupLessonParser.ReadLessons(scheduleDoc).Select(x =>
        {
            x.GroupId = group.Id;
            return x;
        });
    }

    private ICollection<(Group, string)> ReadGroupsList(HtmlDocument document)
    {
        var results = new List<(Group, string)>();
        var accordion = document.DocumentNode.SelectSingleNode("//div[@class='accordion-item']");

        int id = 1;

        while (accordion != null)
        {
            string facultyName = accordion
                .SelectSingleNode(".//button[contains(@class, 'accordion-button')]")
                ?.InnerText;

            var groupsAccordion = accordion.SelectSingleNode(".//div[contains(@class, 'accordion-collapse')]");
            var groups = groupsAccordion?.SelectNodes(".//div[@class='groups-list']/div/a");

            if (groups != null)
            {
                foreach (var groupNode in groups)
                {
                    var newGroup = new Group
                    {
                        Id = id++,
                        GroupName = groupNode.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        FacultyName = facultyName.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                    };
                    results.Add((newGroup, groupNode.GetAttributeValue("href", "")));
                }
            }

            accordion = accordion.SelectSingleNode(".//div[@class='accordion-item']");
        }

        return results;
    }
}