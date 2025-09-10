using DataAccess.Models.Interface;

namespace DataAccess.Repositories.Interfaces;

public interface IModifiedRepository<T>
{
    void PushModifiedEntries(IEnumerable<IModifiedEntry> entries);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}