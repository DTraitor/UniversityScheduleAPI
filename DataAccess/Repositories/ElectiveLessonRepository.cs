using DataAccess.Repositories.Interfaces;
using DataAccess.Domain;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ElectiveLessonRepository : IElectiveLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ElectiveLessonRepository> _logger;

    public ElectiveLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<ElectiveLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<ElectiveLesson>> GetByUserIdAsync(int userId, CancellationToken stoppingToken)
    {
        return await _context.ElectiveLessons.Where(sl => sl.UserId == userId).ToListAsync(stoppingToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}