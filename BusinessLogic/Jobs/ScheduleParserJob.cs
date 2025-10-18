using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class ScheduleParserJob<T, TModifiedEntry> : IHostedService, IDisposable where T : IEntityId where TModifiedEntry : IModifiedEntry
{
    private IServiceProvider _services;
    private readonly ILogger<ScheduleParserJob<T, TModifiedEntry>> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public ScheduleParserJob(
        IServiceProvider services,
        ILogger<ScheduleParserJob<T, TModifiedEntry>> logger)
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
        using var scope = _services.CreateScope();

        var persistentDataRepository = scope.ServiceProvider.GetRequiredService<IPersistentDataRepository>();
        var persistentData = persistentDataRepository.GetData(typeof(T).Name) ?? new PersistentData
        {
            Key = typeof(T).Name,
            Value = DateTimeOffset.UtcNow.ToString("o"),
        };

        if (DateTimeOffset.TryParse(persistentData.Value, out var result) && result > DateTimeOffset.UtcNow)
        {
            return;
        }

        var scheduleReader = scope.ServiceProvider.GetRequiredService<IScheduleReader<T, TModifiedEntry>>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<T>>();
        var modifiedRepository = scope.ServiceProvider.GetRequiredService<IRepository<TModifiedEntry>>();
        var changeHandler = scope.ServiceProvider.GetRequiredService<IChangeHandler<T>>();

        _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTimeOffset.UtcNow.ToString("o"));

        IEnumerable<TModifiedEntry> modifiedEntries;
        ICollection<T> lessons;
        try
        {
            (modifiedEntries, lessons) = await scheduleReader.ReadSchedule(_cancellationTokenSource.Token);
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
                nameof(T));

            persistentData.Value = DateTimeOffset.UtcNow.AddHours(1).ToString("o");
            persistentDataRepository.SetData(persistentData);
            await persistentDataRepository.SaveChangesAsync(_cancellationTokenSource.Token);

            return;
        }

        var previousLessons = await repository.GetAllAsync(_cancellationTokenSource.Token);

        var existing = await changeHandler.HandleChanges(previousLessons, lessons, _cancellationTokenSource.Token);

        HashSet<int> existingHashset = new HashSet<int>(existing.Select(x => x.Id));

        repository.RemoveRange(previousLessons.Where(x => !existingHashset.Contains(x.Id)));

        repository.AddRange(lessons);
        repository.UpdateRange(existing);
        modifiedRepository.AddRange(modifiedEntries);

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