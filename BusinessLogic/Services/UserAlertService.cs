using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserAlertService : IUserAlertService
{
    private IServiceProvider _services;
    private readonly ILogger<UserAlertService> _logger;

    private SynchronizedCollection<UserAlert> _userAlerts = new();

    public UserAlertService(IServiceProvider services, ILogger<UserAlertService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task<ICollection<(UserAlert, User)>> GetAlerts(int batchSize)
    {
        using var scope = _services.CreateScope();

        var userAlertRepository = scope.ServiceProvider.GetRequiredService<IUserAlertRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        try
        {
            var alerts = await userAlertRepository.GetAllLimitAsync(batchSize);
            var users = await userRepository.GetByIdsAsync(alerts.Select(x => x.UserId).ToArray());
            return alerts.Join(users, x => x.UserId, x => x.Id, (alert, user) => (alert, user)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alerts.");
            return [];
        }
    }

    public async Task RemoveProcessedAlerts(ICollection<int> alerts)
    {
        using var scope = _services.CreateScope();

        var userAlertRepository = scope.ServiceProvider.GetRequiredService<IUserAlertRepository>();

        try
        {
            await using var transaction = await userAlertRepository.BeginTransactionAsync();

            await userAlertRepository.RemoveByIdsAsync(alerts);
            await userAlertRepository.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing alerts.");
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

    public ICollection<UserAlert> GetCachedAlerts()
    {
        return _userAlerts.ToArray();
    }

    public void RemoveCachedAlerts(ICollection<UserAlert> alerts)
    {
        foreach (var alert in alerts)
        {
            _userAlerts.Remove(alert);
        }
    }
}