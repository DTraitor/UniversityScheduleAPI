using System.Data.Common;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
    new User Update(User user, DbTransaction transaction);
    Task<User> AddAsync(User user, DbTransaction transaction, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}