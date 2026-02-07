using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserAlertRepository : IRepository<UserAlert>
{
    Task<ICollection<UserAlert>> GetAllLimitAsync(int batchSize);
    Task RemoveByIdsAsync(ICollection<int> alerts);
}