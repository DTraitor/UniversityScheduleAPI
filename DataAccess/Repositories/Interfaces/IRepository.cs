namespace DataAccess.Repositories.Interfaces;

public interface IRepository<T>
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    void AddRange(IEnumerable<T> entities);
    void UpdateRange(IEnumerable<T> entities);
    void RemoveRange(IEnumerable<T> entities);
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}