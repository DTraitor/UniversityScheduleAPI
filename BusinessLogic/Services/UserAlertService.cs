using System.Collections.Generic;
using BusinessLogic.DTO;
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

    public IEnumerable<UserAlert> GetCachedAlerts()
    {
        return _userAlerts.ToArray();
    }

    public void RemoveCachedAlerts(IEnumerable<UserAlert> alerts)
    {
        foreach (var alert in alerts)
        {
            _userAlerts.Remove(alert);
        }
    }
}