using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserAlertService : IHostedService, IUserAlertService
{
    private IServiceProvider _services;
    private readonly ILogger<UserAlertService> _logger;

    private List<UserAlert> _userAlerts = new List<UserAlert>();

    private Timer _timer;
    private CancellationTokenSource _cancellationTokenSource = new();
    private object _executingLock = new();

    public UserAlertService(IServiceProvider services, ILogger<UserAlertService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserAlertService starting...");

        _timer = new Timer(
            ExecuteTimer,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UserAlertService stopping...");

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

        alertRepository.AddRange(_userAlerts);
        _userAlerts.Clear();

        await alertRepository.SaveChangesAsync(_cancellationTokenSource.Token);
    }

    public async Task<IEnumerable<UserAlertDto>> GetAlerts(int batchSize)
    {
        using var scope = _services.CreateScope();

        var userAlertRepository = scope.ServiceProvider.GetRequiredService<IUserAlertRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        try
        {
            var alerts = await userAlertRepository.GetAllLimitAsync(batchSize);
            var users = await userRepository.GetByIdsAsync(alerts.Select(x => x.UserId));
            return alerts.Join(users, x => x.UserId, x => x.Id, (alert, user) => new UserAlertDto
            {
                Id = alert.Id,
                UserTelegramId = user.TelegramId,
                AlertType = alert.AlertType,
                Options = alert.Options,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts.");
            return [];
        }
    }

    public void CreateUserAlert(int userId, UserAlertType type, Dictionary<string, string> options)
    {
        _userAlerts.Add(new UserAlert
        {
            UserId = userId,
            AlertType = type,
            Options = options,
        });
    }

    public async Task RemoveProcessedAlerts(IEnumerable<int> alerts)
    {
        using var scope = _services.CreateScope();

        var userAlertRepository = scope.ServiceProvider.GetRequiredService<IUserAlertRepository>();

        try
        {
            userAlertRepository.RemoveByIds(alerts);
            await userAlertRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing alerts.");
        }
    }
}