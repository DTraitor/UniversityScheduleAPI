namespace DataAccess.Repositories.Interfaces;

public interface IKeyBasedRepository<T> : IRepository<T>
{
    void RemoveByKey(int key);
}