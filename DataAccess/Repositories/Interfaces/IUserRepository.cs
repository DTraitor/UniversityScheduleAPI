using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetByGroupIdsAsync(IEnumerable<int> groupIds);
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds);
    User Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}