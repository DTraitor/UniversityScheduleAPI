using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserModifiedRepository
{
    Task<ICollection<UserModified>> GetNotProcessedAsync(CancellationToken cancellationToken = default);
    void Add(int userId);
    Task RemoveProcessedAsync(ICollection<UserModified> toRemove);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}