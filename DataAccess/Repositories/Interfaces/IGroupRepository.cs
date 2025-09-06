using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupRepository
{
    Task<List<Group>> GetAllAsync(CancellationToken cancellationToken);
    void RemoveRange(IEnumerable<Group> toRemove);
    Task AddRangeAsync(IEnumerable<Group> toUpdate);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}