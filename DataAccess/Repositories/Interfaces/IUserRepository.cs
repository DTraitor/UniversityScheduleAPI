using Common.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<ICollection<User>> GetByIdsAsync(ICollection<int> userIds, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}