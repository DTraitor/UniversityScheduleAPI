using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserAlertService : IUserAlertService
{
    private readonly IUserAlertRepository _userAlertRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserAlertService> _logger;

    public UserAlertService(IUserAlertRepository userAlertRepository, IUserRepository userRepository, ILogger<UserAlertService> logger)
    {
        _userAlertRepository = userAlertRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserAlertDto>> GetAlerts(int batchSize)
    {
        try
        {
            var alerts = await _userAlertRepository.GetAllLimitAsync(batchSize);
            var users = await _userRepository.GetByIdsAsync(alerts.Select(x => x.UserId));
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

    public async Task CreateUserAlert(int userId, UserAlertType type, Dictionary<string, string> options)
    {
        _userAlertRepository.Add(new UserAlert
        {
            UserId = userId,
            AlertType = type,
            Options = options,
        });
        await _userAlertRepository.SaveChangesAsync();
    }

    public async Task RemoveProcessedAlerts(IEnumerable<int> alerts)
    {
        try
        {
            _userAlertRepository.RemoveByIds(alerts);
            await _userAlertRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing alerts.");
        }
    }
}