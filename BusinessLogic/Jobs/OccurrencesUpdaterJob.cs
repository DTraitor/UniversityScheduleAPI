using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class OccurrencesUpdaterJob : IHostedService, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Current date time -> moved date time
    /// </summary>
    private readonly Dictionary<DateTimeOffset, DateTimeOffset> _bachelorSaturdayMove = new()
    {
        { DateTimeOffset.Parse("2025-12-01T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-06T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-02T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-03T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-20T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-04T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-27T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-05T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-04T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-08T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-11T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-09T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-18T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-10T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-25T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-11T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-01T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-12T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-08T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-15T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-15T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-16T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-22T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-17T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-29T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-18T00:00:00.000000+02:00"), DateTimeOffset.Parse("0001-01-01T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-19T00:00:00.000000+02:00"), DateTimeOffset.Parse("0001-01-01T00:00:00.000000+02:00") },
    };

    /// <summary>
    /// Current date time -> moved date time
    /// </summary>
    private readonly Dictionary<DateTimeOffset, DateTimeOffset> _mastersSaturdayMove = new()
    {
        { DateTimeOffset.Parse("2025-09-01T00:00:00.000000+03:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-09-02T00:00:00.000000+03:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-09-03T00:00:00.000000+03:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-09-04T00:00:00.000000+03:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-09-05T00:00:00.000000+03:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-01T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-13T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-02T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-20T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-03T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-09-27T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-04T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-04T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-05T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-11T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-08T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-18T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-09T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-10-25T00:00:00.000000+03:00") },
        { DateTimeOffset.Parse("2025-12-10T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-01T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-11T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-08T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-12T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-15T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-15T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-22T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-16T00:00:00.000000+02:00"), DateTimeOffset.Parse("2025-11-29T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-17T00:00:00.000000+02:00"), DateTimeOffset.Parse("0001-01-01T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-18T00:00:00.000000+02:00"), DateTimeOffset.Parse("0001-01-01T00:00:00.000000+02:00") },
        { DateTimeOffset.Parse("2025-12-19T00:00:00.000000+02:00"), DateTimeOffset.Parse("0001-01-01T00:00:00.000000+02:00") },
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

        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userLessonRepository = scope.ServiceProvider.GetRequiredService<IUserLessonRepository>();
        var userLessonOccurenceRepository = scope.ServiceProvider.GetRequiredService<IUserLessonOccurenceRepository>();

        var lessonsToUpdate = userLessonRepository.GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset.UtcNow.AddDays(90))
            .Where(x => x.OccurrencesCalculatedTill == null || x.OccurrencesCalculatedTill < x.EndTime);

        List<UserLessonOccurrence> userLessonOccurrences = new List<UserLessonOccurrence>();

        DateTimeOffset limit = DateTimeOffset.UtcNow.AddDays(180);

        var users = await userRepository.GetByIdsAsync(lessonsToUpdate.Select(x => x.UserId));
        var groups = await groupRepository.GetByIdsAsync(users.Select(x => x.GroupId.Value));
        var mastersSet = new HashSet<int>(groups.Where(x => x.GroupName[0] == 'М').Select(x => x.Id));

        foreach (var (lesson, user) in lessonsToUpdate.Join(users, x => x.UserId, y => y.Id, (lesson, user) => new Tuple<UserLesson, User>(lesson, user)))
        {
            bool master = mastersSet.Contains(user.GroupId.Value);
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(lesson.TimeZoneId);

            DateTime? latestOccurrence;
            if (lesson.OccurrencesCalculatedTill == null)
                latestOccurrence = lesson.RepeatType == RepeatType.Never
                    ? null
                    : TimeZoneInfo.ConvertTime(lesson.StartTime, timeZone).DateTime;
            else
                latestOccurrence = lesson.RepeatType.GetNextOccurrence(
                    TimeZoneInfo.ConvertTime(lesson.OccurrencesCalculatedTill.Value, timeZone).DateTime,
                    lesson.RepeatCount);

            while (latestOccurrence != null && latestOccurrence < lesson.EndTime && latestOccurrence < limit)
            {
                if (!HandleSaturdays(latestOccurrence.Value, lesson, master, timeZone, out var occurence))
                {
                    occurence = new UserLessonOccurrence
                    {
                        LessonId = lesson.Id,
                        UserId = lesson.UserId,
                        StartTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(lesson.BeginTime)),
                        EndTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(lesson.BeginTime)
                            .Add(lesson.Duration)),
                    };
                }

                userLessonOccurrences.Add(occurence);

                latestOccurrence = lesson.RepeatType.GetNextOccurrence(latestOccurrence.Value, lesson.RepeatCount);
            }

            lesson.OccurrencesCalculatedTill = latestOccurrence == null
                ? null
                : TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value, timeZone);
        }

        if(userLessonOccurrences.Count <= 0)
            return;

        userLessonOccurenceRepository.AddRange(userLessonOccurrences);
        try
        {
            await userLessonRepository.SaveChangesAsync();
            await userLessonOccurenceRepository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is UserLesson)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = await entry.GetDatabaseValuesAsync();

                    if (databaseValues == null)
                    {
                        userLessonOccurenceRepository.RemoveRange(userLessonOccurrences.Where(x => x.LessonId == proposedValues.GetValue<int>("Id")));
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name + "When ");
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        "Don't know how to handle concurrency conflicts for "
                        + entry.Metadata.Name);
                }
            }
        }
    }

    private bool HandleSaturdays(DateTime occurenceDate, UserLesson lesson, bool master, TimeZoneInfo timeZone, out UserLessonOccurrence? userLessonOccurrence)
    {
        var saturdayMoveDictionary = _bachelorSaturdayMove;
        if (master)
            saturdayMoveDictionary = _mastersSaturdayMove;

        userLessonOccurrence = null;
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