using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class OccurrencesUpdaterService : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OccurrencesUpdaterService> _logger;
    private Timer _timer;
    private object _executingLock = new object();

    public OccurrencesUpdaterService(IServiceProvider serviceProvider, ILogger<OccurrencesUpdaterService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OccurrencesUpdaterService starting...");

        _timer = new Timer(
            UpdateOccurrences,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

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
            userLessonOccurenceRepository.SaveChanges();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OccurrencesUpdaterService stopping...");

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