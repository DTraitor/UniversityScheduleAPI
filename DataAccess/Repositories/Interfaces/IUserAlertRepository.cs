using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserAlertRepository : IRepository<UserAlert>
{
    Task<IEnumerable<UserAlert>> GetAllLimitAsync(int batchSize);
    void RemoveByIds(IEnumerable<int> alerts);
}