using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class UserLessonUpdaterJob<T> : IHostedService, IDisposable
{
    private IServiceProvider _services;
    private readonly ILogger<UserLessonUpdaterJob<T>> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public UserLessonUpdaterJob(
        IServiceProvider services,
        ILogger<UserLessonUpdaterJob<T>> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserLessonUpdaterJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserLessonUpdaterJob stopping...");

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

        var modifiedRepository = scope.ServiceProvider.GetRequiredService<IUserModifiedRepository>();
        var lessonUpdater = scope.ServiceProvider.GetRequiredService<IUserLessonUpdaterService<T>>();

        var toProcess =
            (await modifiedRepository.GetNotProcessed(lessonUpdater.ProcessedBy, _cancellationTokenSource.Token));

        await lessonUpdater.ProcessModifiedUser(toProcess.GroupBy(x => x.Key).Select(x => x.First()));

        modifiedRepository.RemoveProcessed(toProcess);

        await modifiedRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}