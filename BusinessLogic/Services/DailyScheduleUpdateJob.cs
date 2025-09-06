using System.Collections.Concurrent;
using BusinessLogic.Mappers;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BusinessLogic.Services.Readers.Interfaces;

namespace BusinessLogic.Services;

public class DailyScheduleUpdateService : BackgroundService
{
    private const string SCHEDULE_API = "https://portal.nau.edu.ua";
    private const string PERSONAL_API = "https://localhost:5001";

    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("01-09-2025");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("31-12-2025");

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
                var (modifiedGroups, removedGroups, modifiedUsers) = await UpdateAllSchedules(stoppingToken);
                _logger.LogInformation("Finished parsing schedule at {Time}", DateTime.Now);

                //TODO: Implement some hashing to prevent unnecessary updates

                _logger.LogInformation("Beginning to upload schedule changes at {Time}", DateTime.Now);
                await UploadChangesToPersonalSchedules(modifiedGroups, removedGroups, modifiedUsers, stoppingToken);
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

    /// <summary>
    /// Populates User specific lessons with latests information
    /// </summary>
    /// <param name="modifiedGroups"></param>
    /// <param name="removedGroups"></param>
    /// <param name="modifiedUsers"></param>
    /// <param name="stoppingToken"></param>
    private async Task UploadChangesToPersonalSchedules(
        IEnumerable<int> modifiedGroups,
        IEnumerable<int> removedGroups,
        IEnumerable<int> modifiedUsers,
        CancellationToken stoppingToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(PERSONAL_API);

        using var scope = _services.CreateScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var electiveLessonRepository = scope.ServiceProvider.GetRequiredService<IElectiveLessonRepository>();
        var scheduleLessonRepository = scope.ServiceProvider.GetRequiredService<IScheduleLessonRepository>();
        var userLessonRepository = scope.ServiceProvider.GetRequiredService<IUserLessonRepository>();
        var userLessonOccurenceRepository = scope.ServiceProvider.GetRequiredService<IUserLessonOccurenceRepository>();

        foreach (var user in await userRepository.GetByGroupIdsAsync(removedGroups))
        {
            user.GroupId = null;
            userRepository.Update(user);
        }

        //Using GroupBy to make sure there are no duplicates
        foreach (var user in (await userRepository.GetByGroupIdsAsync(modifiedGroups.Union(removedGroups)))
                 .Union(await userRepository.GetByIdsAsync(modifiedUsers))
                 .GroupBy(x => x.Id)
                 .Select(x => x.First()))
        {
            userLessonRepository.ClearByUserId(user.Id);
            userLessonOccurenceRepository.ClearByUserId(user.Id);

            if (user.GroupId != null)
            {
                var scheduleLessons = await scheduleLessonRepository.GetByGroupIdAsync(user.GroupId.Value, stoppingToken);
                await userLessonRepository.AddRangeAsync(ScheduleLessonsMapper.Map(scheduleLessons, user.Id, BEGIN_UNIVERSITY_DATE, END_UNIVERSITY_DATE));
            }

            var electiveLessons = await electiveLessonRepository.GetByUserIdAsync(user.Id, stoppingToken);
            await userLessonRepository.AddRangeAsync(ElectiveLessonsMapper.Map(electiveLessons, user.Id, BEGIN_UNIVERSITY_DATE, END_UNIVERSITY_DATE));
        }

        await userRepository.SaveChangesAsync(stoppingToken);
        await electiveLessonRepository.SaveChangesAsync(stoppingToken);
        await scheduleLessonRepository.SaveChangesAsync(stoppingToken);
        await userLessonRepository.SaveChangesAsync(stoppingToken);
        await userLessonOccurenceRepository.SaveChangesAsync(stoppingToken);
    }

    private async Task<(IEnumerable<int>, IEnumerable<int>, IEnumerable<int>)> UpdateAllSchedules(CancellationToken stoppingToken)
    {
        var scheduleGroup = UpdateScheduleGroup(stoppingToken);
        var scheduleElective = UpdateScheduleElective(stoppingToken);

        await Task.WhenAll(scheduleGroup, scheduleElective);

        return (scheduleGroup.Result.Item1, scheduleGroup.Result.Item2, scheduleElective.Result);
    }

    private async Task<IEnumerable<int>> UpdateScheduleElective(CancellationToken stoppingToken)
    {
        return [];
    }

    /// <summary>
    /// Reads out the page with all groups and begins parsing each of them
    /// </summary>
    private async Task<(IEnumerable<int>, IEnumerable<int>)> UpdateScheduleGroup(CancellationToken stoppingToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(SCHEDULE_API);
        var html = await httpClient.GetStringAsync("/schedule/group/list", stoppingToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        using var scope = _services.CreateScope();

        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        var lessonRepository = scope.ServiceProvider.GetRequiredService<IScheduleLessonRepository>();

        var existingGroups = await groupRepository.GetAllAsync(stoppingToken);

        ConcurrentDictionary<string, int> groups = new ConcurrentDictionary<string, int>(existingGroups.Select(g => new KeyValuePair<string, int>(g.GroupName, g.Id)));
        ConcurrentBag<Group> parsedGroups = new ConcurrentBag<Group>();
        int highestId = groups.Values.Max();
        ConcurrentBag<int> modifiedGroupsIds = new ConcurrentBag<int>();

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

            parsedGroups.Add(group.Item1);

            var lessons = await ExtractLessons(group.Item2, group.Item1, ct);
            if (lessons == null)
            {
                return;
            }

            modifiedGroupsIds.Add(group.Item1.Id);

            lessonRepository.RemoveByGroupId(group.Item1.Id);
            await lessonRepository.AddRangeAsync(lessons, ct);
        });

        var parsedGroupsIds = parsedGroups.Select(g => g.Id);
        var removedGroups = existingGroups.Where(g => !parsedGroupsIds.Contains(g.Id));

        foreach (int groupId in modifiedGroupsIds)
        {
            await groupRepository.AddAsync(parsedGroups.First(g => g.Id == groupId));
        }

        foreach (Group group in removedGroups)
        {
            groupRepository.Remove(group);
        }

        await groupRepository.SaveChangesAsync(stoppingToken);
        await lessonRepository.SaveChangesAsync(stoppingToken);

        return (modifiedGroupsIds, removedGroups.Select(g => g.Id));
    }


    /// <summary>
    /// Parses the page of the groups schedule
    /// </summary>
    /// <param name="href">Relative url to the schedule</param>
    /// <param name="group">Group the lessons belongs to</param>
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
}