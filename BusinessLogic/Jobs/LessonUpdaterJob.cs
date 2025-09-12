using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class LessonUpdaterJob<T, TModifiedEntry> : IHostedService, IDisposable where TModifiedEntry : IModifiedEntry
{
    private IServiceProvider _services;
    private readonly ILogger<LessonUpdaterJob<T, TModifiedEntry>> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public LessonUpdaterJob(
        IServiceProvider services,
        ILogger<LessonUpdaterJob<T, TModifiedEntry>> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LessonUpdaterJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LessonUpdaterJob stopping...");

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            UpdateUserLessons().Wait();
        }
    }

    private async Task UpdateUserLessons()
    {
        using var scope = _services.CreateScope();

        var modifiedRepository = scope.ServiceProvider.GetRequiredService<IRepository<TModifiedEntry>>();
        var lessonUpdater = scope.ServiceProvider.GetRequiredService<ILessonUpdaterService<T, TModifiedEntry>>();

        foreach (var modifiedEntries in (await modifiedRepository.GetAllAsync(_cancellationTokenSource.Token)).GroupBy(x => x.Key))
        {
            await lessonUpdater.ProcessModifiedEntry(modifiedEntries.First());

            modifiedRepository.RemoveRange(modifiedEntries);
        }

        await modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);

    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}