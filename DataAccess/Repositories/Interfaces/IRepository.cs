namespace DataAccess.Repositories.Interfaces;

public interface IRepository<T>
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task AddRangeAsync(ICollection<T> entities);
    Task UpdateRangeAsync(ICollection<T> entities);
    Task RemoveRangeAsync(ICollection<T> entities);
    Task<T?> GetByIdAsync(int id);
    Task<ICollection<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}