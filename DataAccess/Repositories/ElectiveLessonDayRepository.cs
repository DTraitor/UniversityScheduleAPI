using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ElectiveLessonDayRepository : IElectiveLessonDayRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ElectiveLessonDayRepository> _logger;

    public ElectiveLessonDayRepository(ScheduleDbContext scheduleDbContext, ILogger<ElectiveLessonDayRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(ElectiveLessonDay entity)
    {
        _context.ElectiveLessonDays.Add(entity);
    }

    public void Update(ElectiveLessonDay entity)
    {
        _context.ElectiveLessonDays.Update(entity);
    }

    public void Delete(ElectiveLessonDay entity)
    {
        _context.ElectiveLessonDays.Remove(entity);
    }

    public void AddRange(IEnumerable<ElectiveLessonDay> entities)
    {
        _context.ElectiveLessonDays.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<ElectiveLessonDay> entity)
    {
        _context.ElectiveLessonDays.UpdateRange(entity);
    }

    public void RemoveRange(IEnumerable<ElectiveLessonDay> entities)
    {
        _context.ElectiveLessonDays.RemoveRange(entities);
    }

    public async Task<ElectiveLessonDay?> GetByIdAsync(int id)
    {
        return await _context.ElectiveLessonDays.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ElectiveLessonDay>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectiveLessonDays.ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }
}