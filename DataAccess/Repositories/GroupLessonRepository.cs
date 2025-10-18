using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class GroupLessonRepository : IGroupLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<GroupLessonRepository> _logger;

    public GroupLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<GroupLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(GroupLesson entity)
    {
        _context.GroupLessons.Add(entity);
    }

    public void Update(GroupLesson entity)
    {
        _context.GroupLessons.Update(entity);
    }

    public void Delete(GroupLesson entity)
    {
        _context.GroupLessons.Remove(entity);
    }

    public void AddRange(IEnumerable<GroupLesson> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<GroupLesson> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<GroupLesson> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<GroupLesson?> GetByIdAsync(int id)
    {
        return await _context.GroupLessons.FirstOrDefaultAsync(x => x.GroupId == id);
    }

    public void RemoveByKey(int key)
    {
        _context.GroupLessons.RemoveRange(_context.GroupLessons.Where(l => l.GroupId == key));
    }

    public async Task<IEnumerable<GroupLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GroupLessons.ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        _context.ExecuteFutureAction();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        _context.ExecuteFutureAction();
        return _context.SaveChanges();
    }

    public async Task<IEnumerable<GroupLesson>> GetByGroupIdsAsync(IEnumerable<int> groupIds, CancellationToken stoppingToken)
    {
        return await _context.GroupLessons.Where(sl => groupIds.Contains(sl.GroupId)).ToListAsync(stoppingToken);
    }
}