using System.Collections.Concurrent;
using DataAcc_ess.Repositories.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reader.Services.Interfaces;

namespace BusinessLogic.Services;

public class DailyScheduleUpdateService : BackgroundService
{
    //readers
    private readonly IElectiveScheduleReader _electiveScheduleReader;
    private readonly IGroupScheduleReader _groupScheduleReader;

    // repositories
    private readonly IGroupRepository _groupRepository;
    private readonly IScheduleLessonRepository _scheduleLessonRepository;
    private readonly IElectiveLessonRepository _electiveLessonRepository;
    private readonly IRemoveGroupRepository _removeGroupRepository;

    // etc
    private readonly ILogger<DailyScheduleUpdateService> _logger;
    private readonly HttpClient _httpClient;

    public DailyScheduleUpdateService(
        IElectiveScheduleReader electiveScheduleReader,
        IGroupScheduleReader groupScheduleReader,
        IGroupRepository groupRepository,
        IScheduleLessonRepository scheduleLessonRepository,
        IElectiveLessonRepository electiveLessonRepository,
        IRemoveGroupRepository removeGroupRepository,
        HttpClient httpClient,
        ILogger<DailyScheduleUpdateService> logger)
    {
        _electiveScheduleReader = electiveScheduleReader;
        _groupScheduleReader = groupScheduleReader;
        _groupRepository = groupRepository;
        _scheduleLessonRepository = scheduleLessonRepository;
        _electiveLessonRepository = electiveLessonRepository;
        _removeGroupRepository = removeGroupRepository;
        _httpClient = httpClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyScheduleUpdateService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int hourSpan = 24 - DateTime.Now.Hour;
            int numberOfHours = hourSpan;

            if (hourSpan == 24)
            {
                _logger.LogInformation("Running daily tasks at {Time}", DateTime.Now);

                await UpdateAllSchedules(stoppingToken);

                numberOfHours = 24;
            }

            _logger.LogInformation("Next run scheduled after {NumberOfHours} hours.", numberOfHours);

            try
            {
                await Task.Delay(TimeSpan.FromHours(numberOfHours), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // service stopping
            }
        }
    }

    private async Task UpdateAllSchedules(CancellationToken stoppingToken)
    {
        var scheduleGroup = UpdateScheduleGroup(stoppingToken);
        var scheduleElective = UpdateScheduleElective(stoppingToken);

        await Task.WhenAll(scheduleGroup, scheduleElective);
    }

    private async Task UpdateScheduleElective(CancellationToken stoppingToken)
    {

    }

    /// <summary>
    /// Reads out the page with all groups and begins parsing each of them
    /// </summary>
    private async Task UpdateScheduleGroup(CancellationToken stoppingToken)
    {
        var html = await _httpClient.GetStringAsync("/schedule/group/list", stoppingToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var newGroups = new ConcurrentBag<Group>();
        var newLessons = new ConcurrentBag<ScheduleLesson>();
        int groupId = 0; // atomic counter

        var accordion = doc.DocumentNode.SelectSingleNode("//div[@class='accordion-item']");

        while (accordion != null)
        {
            string facultyName = accordion
                .SelectSingleNode(".//button[contains(@class, 'accordion-button')]")
                ?.InnerText;

            var groupsAccordion = accordion.SelectSingleNode(".//div[contains(@class, 'accordion-collapse')]");
            var groups = groupsAccordion?.SelectNodes(".//div[@class='groups-list']/div/a");

            if (groups != null)
            {
                // Parallelize group processing
                await Parallel.ForEachAsync(groups, stoppingToken, async (groupNode, ct) =>
                {
                    int id = Interlocked.Increment(ref groupId);

                    var newGroup = new Group
                    {
                        Id = id,
                        GroupName = groupNode.InnerText,
                        FacultyName = facultyName,
                    };
                    newGroups.Add(newGroup);

                    var lessons = await ExtractLessons(groupNode.GetAttributeValue("href", ""), id, ct);
                    foreach (var lesson in lessons)
                    {
                        newLessons.Add(lesson);
                    }
                });
            }

            accordion = accordion.SelectSingleNode(".//div[@class='accordion-item']");
        }

        // Optionally materialize thread-safe collections into lists
        var finalGroups = newGroups.ToList();
        var finalLessons = newLessons.ToList();

        var currentLessons = await _groupRepository.GetAllAsync(stoppingToken);

        var (changed, removed) = SyncGroups(currentLessons, finalGroups);

        foreach (var obj in finalLessons)
        {
            if (changed.TryGetValue(obj.Id, out var value))
            {
                obj.Id = value;
            }
        }

        _groupRepository.RemoveRange(removed);
        _groupRepository.Update(currentLessons);

        await _groupRepository.SaveChangesAsync(stoppingToken);
    }


    /// <summary>
    /// Parses the page of the groups schedule
    /// </summary>
    /// <param name="href">Relative url to the schedule</param>
    /// <param name="groupId">ID of the group the lessons belongs to</param>
    /// <returns></returns>
    private async Task<IEnumerable<ScheduleLesson>> ExtractLessons(string href, int groupId, CancellationToken stoppingToken)
    {
        try
        {
            var scheduleString = await _httpClient.GetStringAsync(href, stoppingToken);

            var scheduleDoc = new HtmlDocument();
            scheduleDoc.LoadHtml(scheduleString);

            return _groupScheduleReader.ReadLessons(scheduleDoc, groupId, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered when processing group HREF: {Href}", href);
            throw;
        }
    }

    /// <summary>
    /// Updates current list of groups with those that were found during parsing while keeping IDs the same
    /// </summary>
    /// <param name="first">Current groups list</param>
    /// <param name="second">New groups list</param>
    /// <returns>Removed groups</returns>
    public static (Dictionary<int, int> ChangedIds, List<Group> RemovedGroups)
        SyncGroups(List<Group> first, List<Group> second)
    {
        var firstLookup = first.ToDictionary(
            g => (g.GroupName, g.FacultyName),
            g => g);

        int nextId = first.Any() ? first.Max(g => g.Id) + 1 : 1;

        var changedIds = new Dictionary<int, int>();

        // Step 1: Match or Add new ones from second list
        foreach (var g in second)
        {
            if (firstLookup.TryGetValue((g.GroupName, g.FacultyName), out var existing))
            {
                if (g.Id != existing.Id)
                {
                    if (g.Id != 0) // only track if second had an Id before
                        changedIds[g.Id] = existing.Id;

                    g.Id = existing.Id;
                }
            }
            else
            {
                int oldId = g.Id;
                g.Id = nextId++;

                if (oldId != 0) // track old → new mapping
                    changedIds[oldId] = g.Id;

                first.Add(new Group
                {
                    Id = g.Id,
                    GroupName = g.GroupName,
                    FacultyName = g.FacultyName
                });
            }
        }

        // Step 2: Find and remove groups not in second
        var secondKeys = new HashSet<(string GroupName, string FacultyName)>(
            second.Select(g => (g.GroupName, g.FacultyName)));

        var removed = first
            .Where(g => !secondKeys.Contains((g.GroupName, g.FacultyName)))
            .ToList();

        first.RemoveAll(g => removed.Contains(g));

        return (changedIds, removed);
    }
}