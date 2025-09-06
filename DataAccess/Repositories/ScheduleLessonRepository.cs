using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ScheduleLessonRepository : IScheduleLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ScheduleLessonRepository> _logger;

    public ScheduleLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<ScheduleLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void AddRange(IEnumerable<ScheduleLesson> toAdd)
    {
        _context.ScheduleLessons.AddRange(toAdd);
    }

    public void RemoveAll()
    {
        _context.ScheduleLessons.RemoveRange(_context.ScheduleLessons);
    }

    public void RemoveByGroupId(int groupId)
    {
        _context.ScheduleLessons.RemoveRange(_context.ScheduleLessons.Where(sl  => sl.GroupId == groupId));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleLesson>> GetByGroupIdAsync(int groupId, CancellationToken stoppingToken)
    {
        return await _context.ScheduleLessons.Where(sl => sl.GroupId == groupId).ToListAsync(stoppingToken);
    }
}