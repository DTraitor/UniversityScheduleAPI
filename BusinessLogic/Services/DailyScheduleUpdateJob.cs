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

public class DailyScheduleUpdateService : IHostedService, IDisposable, IAsyncDisposable
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

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DailyScheduleUpdateService starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DailyScheduleUpdateService stopping...");

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            ParseSchedule().Wait();
        }
    }

    private async Task ParseSchedule()
    {
        using var scope = _services.CreateScope();

        var persistentDataRepository = scope.ServiceProvider.GetRequiredService<IPersistentDataRepository>();
        var persistentData = persistentDataRepository.GetData();

        if (persistentData.NextScheduleParseDateTime.HasValue &&
            persistentData.NextScheduleParseDateTime > DateTimeOffset.Now)
        {
            return;
        }

        _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTime.Now);
        var (modifiedGroups, removedGroups, modifiedUsers) = await UpdateAllSchedules(_cancellationTokenSource.Token);
        _logger.LogInformation("Finished parsing schedule at {Time}", DateTime.Now);

        _logger.LogInformation("Beginning to upload schedule changes at {Time}", DateTime.Now);
        await UploadChangesToPersonalSchedules(modifiedGroups, removedGroups, modifiedUsers, _cancellationTokenSource.Token);
        _logger.LogInformation("Finished uploading schedule at {Time}", DateTime.Now);

        persistentData.NextScheduleParseDateTime = DateTimeOffset.Now.AddHours(24);
        persistentDataRepository.SetData(persistentData);
        await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);
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
        int highestId = groups.Count > 0 ? groups.Values.Max() : 0;
        ConcurrentBag<int> modifiedGroupsIds = new ConcurrentBag<int>();

        object repoLock = new object();

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

            lock (repoLock)
            {
                lessonRepository.RemoveByGroupId(group.Item1.Id);
                lessonRepository.AddRange(lessons);
            }
        });

        var parsedGroupsIds = parsedGroups.Select(g => g.Id);
        var removedGroups = existingGroups.Where(g => !parsedGroupsIds.Contains(g.Id));

        foreach (int groupId in modifiedGroupsIds)
        {
            groupRepository.AddOrUpdate(parsedGroups.First(g => g.Id == groupId));
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

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Dispose();
        await _timer.DisposeAsync();
    }
}