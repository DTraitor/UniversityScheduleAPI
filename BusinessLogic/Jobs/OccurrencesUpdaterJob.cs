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
            _logger.LogDebug("Updating lesson occurrences at: {time}", DateTimeOffset.Now);

            using var scope = _serviceProvider.CreateScope();

            var userLessonRepository = scope.ServiceProvider.GetRequiredService<IUserLessonRepository>();
            var userLessonOccurenceRepository = scope.ServiceProvider.GetRequiredService<IUserLessonOccurenceRepository>();

            var lessonsToUpdate = userLessonRepository.GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset.Now.AddDays(90));

            List<UserLessonOccurrence> userLessonOccurrences = new List<UserLessonOccurrence>();

            DateTimeOffset limit = DateTimeOffset.Now.AddDays(270);

            foreach (var lesson in lessonsToUpdate)
            {
                var occurrence = userLessonOccurenceRepository.GetLatestOccurrence(lesson.Id);
                DateTimeOffset? latestOccurrence;
                if (occurrence == null)
                    latestOccurrence = lesson.RepeatType == RepeatType.Never ? null : lesson.StartTime;
                else
                    latestOccurrence = lesson.RepeatType.GetNextOccurrence(occurrence.StartTime, lesson.RepeatCount);

                while (latestOccurrence != null && latestOccurrence < lesson.EndTime && latestOccurrence < limit)
                {
                    userLessonOccurrences.Add(new UserLessonOccurrence
                    {
                        LessonId = lesson.Id,
                        UserId = lesson.UserId,
                        StartTime = latestOccurrence.Value,
                        EndTime = latestOccurrence.Value.Add(lesson.Duration),
                    });

                    latestOccurrence = lesson.RepeatType.GetNextOccurrence(latestOccurrence.Value, lesson.RepeatCount);
                }

                lesson.OccurrencesCalculatedTill = latestOccurrence;
            }

            userLessonOccurenceRepository.AddRange(userLessonOccurrences);
            try
            {
                userLessonRepository.SaveChanges();
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

                        // Refresh original values to bypass next concurrency check
                        entry.OriginalValues.SetValues(databaseValues);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            "Don't know how to handle concurrency conflicts for "
                            + entry.Metadata.Name);
                    }
                }
            }

            userLessonOccurenceRepository.SaveChanges();
        }
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