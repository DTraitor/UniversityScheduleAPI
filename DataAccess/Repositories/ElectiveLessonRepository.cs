using DataAcc_ess.Repositories.Interfaces;
using DataAccess.Domain;
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
}