namespace DataAccess.Repositories.Interfaces;

public interface IRepository<T>
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    void AddRange(IEnumerable<T> entities);
    void RemoveRange(IEnumerable<T> entities);
    void RemoveByKey(int key);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}