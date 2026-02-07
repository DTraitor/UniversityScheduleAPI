using Common.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IPersistentDataRepository
{
    void SetData(PersistentData persistentData);
    Task<PersistentData?> GetData(string key);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}