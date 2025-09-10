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
        _context.GroupLessons.AddRange(toAdd);
    }

    public void RemoveRange(IEnumerable<GroupLesson> entities)
    {
        _context.GroupLessons.RemoveRange(entities);
    }

    public void RemoveByKey(int key)
    {
        _context.GroupLessons.RemoveRange(_context.GroupLessons.Where(l => l.GroupId == key));
    }

    public void RemoveAll()
    {
        _context.GroupLessons.RemoveRange(_context.GroupLessons);
    }

    public void RemoveByGroupId(int groupId)
    {
        _context.GroupLessons.RemoveRange(_context.GroupLessons.Where(sl  => sl.GroupId == groupId));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<IEnumerable<GroupLesson>> GetByGroupIdAsync(int groupId, CancellationToken stoppingToken)
    {
        return await _context.GroupLessons.Where(sl => sl.GroupId == groupId).ToListAsync(stoppingToken);
    }
}