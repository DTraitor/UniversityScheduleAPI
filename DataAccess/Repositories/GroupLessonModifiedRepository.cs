using DataAccess.Models;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories;

public class GroupLessonModifiedRepository : IRepository<GroupLessonModified>
{
    public void Add(GroupLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void Update(GroupLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(GroupLessonModified entity)
    {
        throw new NotImplementedException();
    }

    public void AddRange(IEnumerable<GroupLessonModified> entities)
    {
        throw new NotImplementedException();
    }

    public void RemoveRange(IEnumerable<GroupLessonModified> entities)
    {
        throw new NotImplementedException();
    }

    public Task<GroupLessonModified?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GroupLessonModified>> GetAllAsync(CancellationToken cancellationToken = default)
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