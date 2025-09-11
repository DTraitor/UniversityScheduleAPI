using BusinessLogic.Services.Interfaces;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class ScheduleParserJob<T, TModifiedEntry> : IHostedService, IDisposable where TModifiedEntry : IModifiedEntry
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
            ParseSchedule().Wait();
        }
    }

    private async Task ParseSchedule()
    {
        using var scope = _services.CreateScope();

        var persistentDataRepository = scope.ServiceProvider.GetRequiredService<IPersistentDataRepository>();
        var persistentData = persistentDataRepository.GetData();

        if (persistentData.NextScheduleParseDateTime[typeof(T).Name] > DateTimeOffset.Now)
        {
            return;
        }

        var scheduleReader = scope.ServiceProvider.GetRequiredService<IScheduleReader<T, TModifiedEntry>>();
        var repository = scope.ServiceProvider.GetRequiredService<IKeyBasedRepository<T>>();
        var modifiedRepository = scope.ServiceProvider.GetRequiredService<IRepository<TModifiedEntry>>();

        _logger.LogInformation("Beginning daily parsing of the schedule at {Time}", DateTime.Now);

        var (modifiedEntries, lessons) = await scheduleReader.ReadSchedule(_cancellationTokenSource.Token);

        foreach (var modifiedEntry in modifiedEntries)
        {
            repository.RemoveByKey(modifiedEntry.Key);
        }
        repository.AddRange(lessons);
        modifiedRepository.AddRange(modifiedEntries);

        await modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);
        await repository.SaveChangesAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Finished parsing schedule at {Time}", DateTime.Now);

        persistentData.NextScheduleParseDateTime[typeof(T).Name] = DateTimeOffset.Now.AddHours(24);
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