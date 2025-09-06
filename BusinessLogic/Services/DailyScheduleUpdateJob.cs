using System.Collections.Concurrent;
using DataAcc_ess.Repositories.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reader.Services.Interfaces;

namespace BusinessLogic.Services;

public class DailyScheduleUpdateService : BackgroundService
{
    private const string SCHEDULE_API = "https://portal.nau.edu.ua";

    //readers
    private readonly IGroupsListReader  _groupsListReader;
    private readonly IElectiveScheduleReader _electiveScheduleReader;
    private readonly IGroupScheduleReader _groupScheduleReader;

    // etc
    private IServiceProvider _services;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DailyScheduleUpdateService> _logger;

    public DailyScheduleUpdateService(
        IGroupsListReader  groupsListReader,
        IElectiveScheduleReader electiveScheduleReader,
        IGroupScheduleReader groupScheduleReader,
        IServiceProvider services,
        IHttpClientFactory httpClientFactory,
        ILogger<DailyScheduleUpdateService> logger)
    {
        _groupsListReader = groupsListReader;
        _electiveScheduleReader = electiveScheduleReader;
        _groupScheduleReader = groupScheduleReader;
        _services = services;
        _httpClientFactory = httpClientFactory;
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
                _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTime.Now);
                await UpdateAllSchedules(stoppingToken);
                _logger.LogInformation("Finished parsing schedule at {Time}", DateTime.Now);

                //TODO: Implement some hashing to prevent unnecessary updates

                _logger.LogInformation("Beginning to upload schedule changes at {Time}", DateTime.Now);
                await UploadChangesToPersonalAPI(stoppingToken);
                _logger.LogInformation("Finished uploading schedule at {Time}", DateTime.Now);

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

    private async Task UploadChangesToPersonalAPI(CancellationToken stoppingToken)
    {

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
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(SCHEDULE_API);
        var html = await httpClient.GetStringAsync("/schedule/group/list", stoppingToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        using var scope = _services.CreateScope();

        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        var lessonRepository = scope.ServiceProvider.GetRequiredService<IScheduleLessonRepository>();

        var existingGroups = await groupRepository.GetAllAsync(stoppingToken);

        ConcurrentDictionary<string, int> groups = new ConcurrentDictionary<string, int>(existingGroups.Select(g => new KeyValuePair<string, int>(g.GroupName, g.Id)));
        int highestId = groups.Values.Max();

        await Parallel.ForEachAsync(_groupsListReader.ReadGroupsList(doc), stoppingToken, async (group, ct) =>
        {
            if (groups.TryGetValue(group.Item1.GroupName, out var id))
            {
                group.Item1.Id = id;
            }
            else
            {
                Interlocked.Increment(ref highestId);
                group.Item1.Id = highestId;
                if (!groups.TryAdd(group.Item1.GroupName, highestId))
                {
                    _logger.LogError("Couldn't add group {GroupName} to dictionary. Id {GroupId}", group.Item1.GroupName, group.Item1.Id);
                }
            }

            var lessons = await ExtractLessons(group.Item2, group.Item1, ct);
        });


        // Optionally materialize thread-safe collections into lists
        var finalGroups = newGroups.ToList();
        var finalLessons = newLessons.ToList();


        var currentLessons = await groupRepository.GetAllAsync(stoppingToken);

        var (changed, removed) = SyncGroups(currentLessons, finalGroups);

        foreach (var obj in finalLessons)
        {
            if (changed.TryGetValue(obj.Id, out var value))
            {
                obj.Id = value;
            }
        }

        groupRepository.RemoveRange(removed);
        await groupRepository.AddRangeAsync(currentLessons);
        await groupRepository.SaveChangesAsync(stoppingToken);

        lessonRepository.RemoveAll();
        await lessonRepository.AddRangeAsync(finalLessons, stoppingToken);
        await lessonRepository.SaveChangesAsync(stoppingToken);
    }


    /// <summary>
    /// Parses the page of the groups schedule
    /// </summary>
    /// <param name="href">Relative url to the schedule</param>
    /// <param name="groupId">ID of the group the lessons belongs to</param>
    /// <param name="stoppingToken">Cancellation token</param>
    /// <returns></returns>
    private async Task<IEnumerable<ScheduleLesson>?> ExtractLessons(string href, Group group, CancellationToken stoppingToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(SCHEDULE_API);
            var scheduleString = await httpClient.GetStringAsync(href, stoppingToken);

            var scheduleDoc = new HtmlDocument();
            scheduleDoc.LoadHtml(scheduleString);

            return _groupScheduleReader.ReadLessons(scheduleDoc, group, stoppingToken);
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
                    if (g.Id != 0) // only track if second had an ID before
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