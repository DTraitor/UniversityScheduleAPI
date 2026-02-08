using Common.Enums;
using Common.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUserAlertService
{
    Task<ICollection<(UserAlert, User)>> GetAlerts(int batchSize);
    Task RemoveProcessedAlerts(ICollection<int> alerts);

    void CreateUserAlert(int userId, UserAlertType type, Dictionary<string, string> options);
    ICollection<UserAlert> GetCachedAlerts();
    void RemoveCachedAlerts(ICollection<UserAlert> alerts);
}