using System.Collections.Concurrent;
using System.Text.Json;
using BusinessLogic.Parsing.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class ScheduleParserJob : IHostedService, IDisposable
{
    private IServiceProvider _services;
    private readonly ILogger<ScheduleParserJob> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public ScheduleParserJob(
        IServiceProvider services,
        ILogger<ScheduleParserJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduleParserJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduleParserJob stopping...");

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            ParseSchedule().GetAwaiter().GetResult();
        }
    }

    private async Task ParseSchedule()
    {
        return;
        using var scope = _services.CreateScope();

        var persistentDataRepository = scope.ServiceProvider.GetRequiredService<IPersistentDataRepository>();
        var persistentData = persistentDataRepository.GetData(nameof(LessonSource)) ?? new PersistentData
        {
            Key = nameof(LessonSource),
            Value = DateTimeOffset.UtcNow.ToString("o"),
        };

        if (DateTimeOffset.TryParse(persistentData.Value, out var result) && result > DateTimeOffset.UtcNow)
        {
            return;
        }

        var scheduleReader = scope.ServiceProvider.GetRequiredService<IScheduleReader>();
        var sourcesRepository = scope.ServiceProvider.GetRequiredService<ILessonSourceRepository>();
        var repository = scope.ServiceProvider.GetRequiredService<ILessonEntryRepository>();
        var modifiedRepository = scope.ServiceProvider.GetRequiredService<ILessonSourceModifiedRepository>();
        var changeHandler = scope.ServiceProvider.GetRequiredService<IChangeHandler>();

        _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTimeOffset.UtcNow.ToString("o"));

        var previousLessons = await repository.GetAllAsync(_cancellationTokenSource.Token);

        ConcurrentBag<int> updatedSources = new();
        ConcurrentBag<LessonEntry> newEntries = new();

        try
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10,
                CancellationToken = _cancellationTokenSource.Token
            };

            await Parallel.ForEachAsync(await sourcesRepository.GetAllAsync(), options, async (source, token) =>
            {
                ICollection<LessonEntry>? lessons;
                try
                {
                    lessons = await scheduleReader.ReadSchedule(source, token);
                    if (lessons == null)
                        return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while parsing schedule for source id: '{SourceId}'", source.Id);
                    return;
                }

                updatedSources.Add(source.Id);
                foreach (var lessonEntry in lessons)
                {
                    newEntries.Add(lessonEntry);
                }
            });
        }
        catch (AggregateException ex)
        {
            _logger.LogError(
                ex.InnerExceptions.FirstOrDefault(),
                "Error reading schedule at {Time}",
                DateTimeOffset.UtcNow.ToString("o")
            );

            persistentData.Value = DateTimeOffset.UtcNow.AddHours(1).ToString("o");
            persistentDataRepository.SetData(persistentData);
            await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);

            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error reading schedule at {Time}, {ParseServiceType}",
                DateTimeOffset.UtcNow.ToString("o"),
                nameof(LessonSource));

            persistentData.Value = DateTimeOffset.UtcNow.AddHours(1).ToString("o");
            persistentDataRepository.SetData(persistentData);
            await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);

            return;
        }

        previousLessons = previousLessons.Where(x => updatedSources.Contains(x.SourceId)).ToList();
        var currentLessons = newEntries.ToList();

        var existing = await changeHandler.HandleChanges(previousLessons, currentLessons, _cancellationTokenSource.Token);

        HashSet<int> existingHashset = new HashSet<int>(existing.Select(x => x.Id));

        repository.RemoveRange(previousLessons.Where(x => !existingHashset.Contains(x.Id)));

        repository.AddRange(currentLessons);
        repository.UpdateRange(existing);
        modifiedRepository.AddRange(updatedSources.Select(x => new LessonSourceModified{ SourceId = x }));

        await repository.SaveChangesAsync(_cancellationTokenSource.Token);
        await modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Finished parsing schedule at {Time}", DateTimeOffset.UtcNow.ToString("o"));

        persistentData.Value = DateTimeOffset.UtcNow.AddHours(6).ToString("o");
        persistentDataRepository.SetData(persistentData);
        await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}