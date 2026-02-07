using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class UserAlertJob : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<UserAlertJob> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public UserAlertJob(
        IServiceProvider services,
        IUserAlertService userAlertService,
        ILogger<UserAlertJob> logger)
    {
        _services = services;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserAlertJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(5));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserAlertJob stopping...");

        ExecuteTimer(null);

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            PushAlerts().GetAwaiter().GetResult();
        }
    }

    private async Task PushAlerts()
    {
        using var scope = _services.CreateScope();

        var alertRepository = scope.ServiceProvider.GetRequiredService<IUserAlertRepository>();

        var alerts = _userAlertService.GetCachedAlerts();
        alertRepository.AddRangeAsync(alerts);
        _userAlertService.RemoveCachedAlerts(alerts);

        await alertRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}