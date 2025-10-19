using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IUserModifiedRepository
{
    Task<IEnumerable<UserModified>> GetNotProcessed(CancellationToken cancellationToken = default);
    void Add(int userId);
    void RemoveProcessed(IEnumerable<UserModified> toRemove);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}