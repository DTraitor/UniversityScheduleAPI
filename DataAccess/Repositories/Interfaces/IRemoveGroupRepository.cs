using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IRemoveGroupRepository
{
    Task<List<Group>> GetAllAsync(CancellationToken cancellationToken);
    void AddRange(IEnumerable<Group> toRemove);
    void RemoveRange(IEnumerable<Group> toRemove);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}