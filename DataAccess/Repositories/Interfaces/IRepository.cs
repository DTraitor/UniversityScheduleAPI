using System.Data.Common;

namespace DataAccess.Repositories.Interfaces;

public interface IRepository<T>
{
    void Add(T entity, DbTransaction transaction);
    void Update(T entity, DbTransaction transaction);
    void Delete(T entity, DbTransaction transaction);
    void AddRange(IEnumerable<T> entities, DbTransaction transaction);
    void UpdateRange(IEnumerable<T> entities, DbTransaction transaction);
    void RemoveRange(IEnumerable<T> entities, DbTransaction transaction);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}