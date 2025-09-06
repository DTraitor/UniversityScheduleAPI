using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPersistentDataRepository
{
    void SetData(PersistentData persistentData);
    PersistentData GetData();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}