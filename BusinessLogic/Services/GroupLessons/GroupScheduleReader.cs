using System.Collections.Concurrent;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.GroupLessons;

public class GroupScheduleReader : IScheduleReader<GroupLesson, GroupLessonModified>
{
    private const string SCHEDULE_API = "https://portal.nau.edu.ua";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGroupRepository  _groupRepository;
    private readonly IScheduleParser<GroupLesson> _groupLessonParser;
    private readonly ILogger<GroupScheduleReader> _logger;

    public GroupScheduleReader(
        IHttpClientFactory httpClientFactory,
        IGroupRepository groupRepository,
        IScheduleParser<GroupLesson> groupLessonParser,
        ILogger<GroupScheduleReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _groupRepository = groupRepository;
        _groupLessonParser = groupLessonParser;
        _logger = logger;
    }

    public async Task<(IEnumerable<GroupLessonModified>, IEnumerable<GroupLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(SCHEDULE_API);

        var html = await httpClient.GetStringAsync("/schedule/group/list", cancellationToken);

        var groupsList = new HtmlDocument();
        groupsList.LoadHtml(html);

        var groupsData = ReadGroupsList(groupsList);
        ConcurrentBag<GroupLesson> groupLessons = new ConcurrentBag<GroupLesson>();
        ConcurrentBag<GroupLessonModified> groupsModifications = new ConcurrentBag<GroupLessonModified>();

        await Parallel.ForEachAsync(groupsData, cancellationToken, async (groupData, ct) =>
        {
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

        var removedGroups = (await _groupRepository.GetAllAsync(cancellationToken))
            .Where(c => !newGroupNames.Contains(c.GroupName));

        _groupRepository.RemoveRange(removedGroups);

        await _groupRepository.SaveChangesAsync(cancellationToken);

        return (groupsModifications, groupLessons);
    }

    private async Task<IEnumerable<GroupLesson>?> FetchGroupScheduleAsync(string href, Group group, CancellationToken stoppingToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(SCHEDULE_API);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered when processing group HREF: {Href}", href);
            throw;
        }
    }

    private ICollection<(Group, string)> ReadGroupsList(HtmlDocument document)
    {
        var results = new List<(Group, string)>();
        var accordion = document.DocumentNode.SelectSingleNode("//div[@class='accordion-item']");

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