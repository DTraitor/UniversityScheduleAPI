using BusinessLogic.DTO;
using DataAccess.Enums;
using DataAccess.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUserAlertService
{
    Task<IEnumerable<UserAlertDto>> GetAlerts(int batchSize);
    void CreateUserAlert(int userId, UserAlertType type, Dictionary<string, string> options);
    Task RemoveProcessedAlerts(IEnumerable<int> alerts);
    IEnumerable<UserAlert> GetCachedAlerts();
    void RemoveCachedAlerts(IEnumerable<UserAlert> alerts);
}