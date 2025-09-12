using DataAccess.Models;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories;

public class ElectiveLessonModifiedRepository : IRepository<ElectiveLessonModified>
{
    public void Add(ElectiveLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void Update(ElectiveLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(ElectiveLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void AddRange(IEnumerable<ElectiveLessonModified> entities)
    {
        throw new NotImplementedException();
    }

    public void RemoveRange(IEnumerable<ElectiveLessonModified> entities)
    {
        throw new NotImplementedException();
    }

    public Task<ElectiveLessonModified?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ElectiveLessonModified>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int SaveChanges()
    {
        throw new NotImplementedException();
    }
}