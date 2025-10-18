using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class GroupLessonModifiedRepository : IRepository<GroupLessonModified>
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<GroupLessonModifiedRepository> _logger;

    public GroupLessonModifiedRepository(ScheduleDbContext scheduleDbContext, ILogger<GroupLessonModifiedRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(GroupLessonModified entity)
    {
        _context.GroupLessonModifications.Add(entity);
    }

    public void Update(GroupLessonModified entity)
    {
        _context.GroupLessonModifications.Update(entity);
    }

    public void Delete(GroupLessonModified entity)
    {
        _context.GroupLessonModifications.Remove(entity);
    }

    public void AddRange(IEnumerable<GroupLessonModified> entities)
    {
        _context.FutureAction(x => x.BulkInsert(entities));
    }

    public void UpdateRange(IEnumerable<GroupLessonModified> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<GroupLessonModified> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<GroupLessonModified?> GetByIdAsync(int id)
    {
        return await _context.GroupLessonModifications.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<GroupLessonModified>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GroupLessonModifications.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ExecuteFutureAction();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        _context.ExecuteFutureAction();
        return _context.SaveChanges();
    }
}