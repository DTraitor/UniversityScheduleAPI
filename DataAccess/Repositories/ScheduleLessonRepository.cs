using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
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

    public async Task AddRangeAsync(IEnumerable<ScheduleLesson> toAdd, CancellationToken stoppingToken)
    {
        await _context.ScheduleLessons.AddRangeAsync(toAdd, stoppingToken);
    }

    public void RemoveAll()
    {
        _context.ScheduleLessons.RemoveRange(_context.ScheduleLessons);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}