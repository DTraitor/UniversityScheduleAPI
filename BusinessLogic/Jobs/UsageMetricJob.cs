using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Jobs;

public class UserMetricJob : IHostedService, IDisposable
{
    private IServiceProvider _services;
    private IUsageMetricService _usageService;
    private readonly ILogger<UserMetricJob> _logger;

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public UserMetricJob(
        IServiceProvider services,
        IUsageMetricService usageService,
        ILogger<UserMetricJob> logger)
    {
        _services = services;
        _usageService = usageService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserMetricJob starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserMetricJob stopping...");

        _cancellationTokenSource.Cancel();
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void ExecuteTimer(object? state)
    {
        lock (_executingLock)
        {
            PushMetrics().GetAwaiter().GetResult();
        }
    }

    private async Task PushMetrics()
    {
        using var scope = _services.CreateScope();

        var metricRepository = scope.ServiceProvider.GetRequiredService<IUsageMetricRepository>();

        metricRepository.AddRangeAsync(_usageService.GetUsages());

        await metricRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _timer.Dispose();
    }
}