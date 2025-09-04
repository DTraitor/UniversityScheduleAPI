using DataAccess.Domain;
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
}