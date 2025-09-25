using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class OccurrencesUpdaterJob : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly DateTime _saturdayMoveStart = DateTime.Parse("2025-12-01T00:00:00.000000+02:00");
    private readonly DateTime _saturdayMoveEnd = DateTime.Parse("2025-12-14T23:59:59.999999+02:00");
    private readonly DateTime _saturdayMoveToStart = DateTime.Parse("2025-09-06T00:00:00.000000+03:00");

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
            UpdateOccurrences,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));

        return Task.CompletedTask;
    }

    private void UpdateOccurrences(object? state)
    {
        lock (_executingLock)
        {
            _logger.LogDebug("Updating lesson occurrences at: {time}", DateTimeOffset.UtcNow.ToString("o"));

            using var scope = _serviceProvider.CreateScope();

            var userLessonRepository = scope.ServiceProvider.GetRequiredService<IUserLessonRepository>();
            var userLessonOccurenceRepository = scope.ServiceProvider.GetRequiredService<IUserLessonOccurenceRepository>();

            var lessonsToUpdate = userLessonRepository.GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset.UtcNow.AddDays(90))
                .Where(x => x.OccurrencesCalculatedTill == null || x.OccurrencesCalculatedTill < x.EndTime);

            List<UserLessonOccurrence> userLessonOccurrences = new List<UserLessonOccurrence>();

            DateTimeOffset limit = DateTimeOffset.UtcNow.AddDays(180);

            foreach (var lesson in lessonsToUpdate)
            {
                TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(lesson.TimeZoneId);

                DateTime? latestOccurrence;
                if (lesson.OccurrencesCalculatedTill == null)
                    latestOccurrence = lesson.RepeatType == RepeatType.Never ? null : TimeZoneInfo.ConvertTime(lesson.StartTime, timeZone).DateTime;
                else
                    latestOccurrence = lesson.RepeatType.GetNextOccurrence(TimeZoneInfo.ConvertTime(lesson.OccurrencesCalculatedTill.Value, timeZone).DateTime, lesson.RepeatCount);

                while (latestOccurrence != null && latestOccurrence < lesson.EndTime && latestOccurrence < limit)
                {
                    if (!HandleSaturdays(latestOccurrence.Value, lesson, out var occurence))
                    {
                        occurence = new UserLessonOccurrence
                        {
                            LessonId = lesson.Id,
                            UserId = lesson.UserId,
                            StartTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(lesson.BeginTime)),
                            EndTime = TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value.Add(lesson.BeginTime).Add(lesson.Duration)),
                        };
                    }

                    userLessonOccurrences.Add(occurence);

                    latestOccurrence = lesson.RepeatType.GetNextOccurrence(latestOccurrence.Value, lesson.RepeatCount);
                }

                lesson.OccurrencesCalculatedTill = latestOccurrence == null? null : TimeZoneInfo.ConvertTimeToUtc(latestOccurrence.Value, timeZone);
            }

            if(userLessonOccurrences.Count <= 0)
                return;

            userLessonOccurenceRepository.AddRange(userLessonOccurrences);
            try
            {
                userLessonRepository.SaveChanges();
                userLessonOccurenceRepository.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is UserLesson)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = entry.GetDatabaseValues();

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
    }

    public bool HandleSaturdays(DateTime occurenceDate, UserLesson lesson, out UserLessonOccurrence? userLessonOccurrence)
    {
        userLessonOccurrence = null;
        if (occurenceDate < _saturdayMoveStart || occurenceDate > _saturdayMoveEnd)
            return false;

        var diff = occurenceDate - _saturdayMoveStart;

        var movedDate = _saturdayMoveToStart.AddDays(7 * (diff.Days));

        userLessonOccurrence = new UserLessonOccurrence
        {
            LessonId = lesson.Id,
            UserId = lesson.UserId,
            StartTime = TimeZoneInfo.ConvertTimeToUtc(movedDate.Add(lesson.BeginTime)),
            EndTime = TimeZoneInfo.ConvertTimeToUtc(movedDate.Add(lesson.BeginTime).Add(lesson.Duration)),
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