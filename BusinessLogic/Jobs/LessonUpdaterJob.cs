using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models.Interface;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class LessonUpdaterJob : IHostedService, IDisposable
{
    private IServiceProvider _services;
    private readonly ILogger<LessonUpdaterJob> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public LessonUpdaterJob(
        IServiceProvider services,
        ILogger<LessonUpdaterJob> logger)
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
            UpdateUserLessons().GetAwaiter().GetResult();
        }
    }

    private async Task UpdateUserLessons()
    {
        using var scope = _services.CreateScope();

        var modifiedRepository = scope.ServiceProvider.GetRequiredService<ILessonSourceModifiedRepository>();
        var lessonUpdater = scope.ServiceProvider.GetRequiredService<ILessonUpdaterService>();

        var toProcess =
            (await modifiedRepository.GetAllAsync(_cancellationTokenSource.Token));

        await lessonUpdater.ProcessModifiedEntry(toProcess.GroupBy(x => x.SourceId).Select(x => x.First()));

        modifiedRepository.RemoveRange(toProcess);

        await modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);

    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}