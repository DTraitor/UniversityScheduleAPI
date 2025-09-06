using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetByGroupIdsAsync(IEnumerable<int> groupIds);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}