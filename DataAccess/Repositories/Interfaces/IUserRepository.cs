using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> GetByGroupIdsAsync(IEnumerable<int> groupIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    new User Update(User user);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}