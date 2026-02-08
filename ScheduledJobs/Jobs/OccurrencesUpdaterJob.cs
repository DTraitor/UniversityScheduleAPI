using System.Diagnostics.CodeAnalysis;
using BusinessLogic.Helpers;
using Common.Enums;
using Common.Models;
using Common.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScheduledJobs.Jobs;

public class OccurrencesUpdaterJob : IHostedService, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Current date time -> moved date time
    /// </summary>
    private readonly Dictionary<DateTimeOffset, DateTimeOffset> _bachelorSaturdayMove = new()
    {
        { DateTimeOffset.Parse("2026-01-19T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-07T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-20T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-14T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-21T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-21T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-22T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-28T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-23T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-07T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-26T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-14T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-27T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-21T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-28T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-28T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-29T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-04-04T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2026-01-30T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-04-11T00:00:00.000000+03:00") },
    };

    /// <summary>
    /// Current date time -> moved date time
    /// </summary>
    private readonly Dictionary<DateTimeOffset, DateTimeOffset> _mastersSaturdayMove = new()
    {
        { DateTimeOffset.Parse("2026-01-19T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-07T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-20T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-14T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-21T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-21T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-22T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-02-28T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-23T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-07T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-26T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-14T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-27T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-21T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-28T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-03-28T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2026-01-29T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-04-04T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2026-01-30T00:00:00.000000+02:00"), DateTimeOffset.Parse("2026-04-11T00:00:00.000000+03:00") },
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OccurrencesUpdaterJob> _logger;
    private Timer _timer;
    private object _executingLock = new object();

    public OccurrencesUpdaterJob(IServiceProvider serviceProvider, ILogger<OccurrencesUpdaterJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OccurrencesUpdaterJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));

        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            UpdateOccurrences().GetAwaiter().GetResult();
        }
    }

    private async Task UpdateOccurrences()
    {
        _logger.LogDebug("Updating lesson occurrences at: {time}", DateTimeOffset.UtcNow.ToString("o"));

        using var scope = _serviceProvider.CreateScope();

        var selectedLessonSourceRepository = scope.ServiceProvider.GetRequiredService<ISelectedLessonSourceRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userLessonRepository = scope.ServiceProvider.GetRequiredService<IUserLessonRepository>();
        var userLessonOccurenceRepository = scope.ServiceProvider.GetRequiredService<IUserLessonOccurenceRepository>();

        var lessonsToUpdate = await
            userLessonRepository.GetWithOccurrencesCalculatedDateLessThanAsync(DateTimeOffset.UtcNow.AddDays(90));

        if(!lessonsToUpdate.Any())
            return;

        List<UserLessonOccurrence> userLessonOccurrences = new List<UserLessonOccurrence>();

        DateTimeOffset limit = DateTimeOffset.UtcNow.AddDays(180);

        var users = await userRepository.GetByIdsAsync(lessonsToUpdate.Select(x => x.UserId).ToList());
        var sources = await selectedLessonSourceRepository.GetByUserIdsAndSourceType(users.Select(x => x.Id).ToList(), LessonSourceType.Group);
        var mastersSet = new HashSet<int>(sources.Where(x => x.SourceName[0] == 'М').Select(x => x.Id));

        foreach (var (lesson, user) in lessonsToUpdate.Join(users, x => x.UserId, y => y.Id, (lesson, user) => new Tuple<UserLesson, User>(lesson, user)))
        {
            bool master = mastersSet.Contains(sources.FirstOrDefault(x => x.UserId == user.Id)?.Id ?? -2);
            userLessonOccurrences.AddRange(GetOccurrences(master, lesson, limit));
        }

        if(userLessonOccurrences.Count <= 0)
            return;

        await using var transaction = await userLessonOccurenceRepository.BeginTransactionAsync();

        // Might add try catch later here
        await userLessonOccurenceRepository.AddRangeAsync(userLessonOccurrences);

        await userLessonRepository.SaveChangesAsync();
        await userLessonOccurenceRepository.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private List<UserLessonOccurrence> GetOccurrences(bool isMasterSource, UserLesson userLesson, DateTimeOffset generateLimit)
    {
        List<UserLessonOccurrence> occurrences = new List<UserLessonOccurrence>();

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(userLesson.TimeZoneId);

        DateTime? latestOccurrence;
        if (userLesson.OccurrencesCalculatedTill == null)
            latestOccurrence = TimeZoneInfo.ConvertTime(userLesson.StartTime, timeZone).DateTime;
        else
            latestOccurrence = userLesson.RepeatType.GetNextOccurrence(
                TimeZoneInfo.ConvertTime(userLesson.OccurrencesCalculatedTill.Value, timeZone).DateTime,
                userLesson.RepeatCount);

        DateTime endTimeWithZone = TimeZoneInfo.ConvertTime(userLesson.EndTime, timeZone).DateTime;

        while (latestOccurrence != null && latestOccurrence < endTimeWithZone && latestOccurrence < generateLimit)
        {
            if (!HandleSaturdays(latestOccurrence.Value, userLesson, isMasterSource, timeZone, out var occurence))
            {
                occurence = new UserLessonOccurrence
                {
                    LessonId = userLesson.Id,
                    UserId = userLesson.UserId,
                    StartTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(userLesson.BeginTime)),
                    EndTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(userLesson.BeginTime)
                        .Add(userLesson.Duration)),
                };
            }

            occurrences.Add(occurence);

            userLesson.OccurrencesCalculatedTill = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value, timeZone);

            latestOccurrence = userLesson.RepeatType.GetNextOccurrence(latestOccurrence.Value, userLesson.RepeatCount);
        }

        if(latestOccurrence != null)
            userLesson.OccurrencesCalculatedTill = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value, timeZone);

        return occurrences;
    }

    private bool HandleSaturdays(DateTime occurenceDate, UserLesson lesson, bool master,
        TimeZoneInfo timeZone, [NotNullWhen(true)] out UserLessonOccurrence? userLessonOccurrence)
    {
        userLessonOccurrence = null;

        if((lesson.SelectedLessonSourceType & SelectedLessonSourceType.OneTimeOccurence) == SelectedLessonSourceType.OneTimeOccurence)
            return false;

        var saturdayMoveDictionary = _bachelorSaturdayMove;
        if (master)
            saturdayMoveDictionary = _mastersSaturdayMove;

        if (!saturdayMoveDictionary.TryGetValue(TimeZoneInfo.ConvertTimeToUtc(occurenceDate, timeZone), out var movedDateTime))
            return false;

        userLessonOccurrence = new UserLessonOccurrence
        {
            LessonId = lesson.Id,
            UserId = lesson.UserId,
            StartTime = movedDateTime.Add(lesson.BeginTime).ToUniversalTime(),
            EndTime = movedDateTime.Add(lesson.BeginTime).Add(lesson.Duration).ToUniversalTime(),
        };

        return true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OccurrencesUpdaterJob stopping...");

        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
    }
}