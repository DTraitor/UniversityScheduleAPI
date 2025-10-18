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
        _context.FutureAction(x => x.BulkInsert(entities));
    }

    public void UpdateRange(IEnumerable<ElectiveLessonDay> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<ElectiveLessonDay> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<ElectiveLessonDay?> GetByIdAsync(int id)
    {
        return await _context.ElectiveLessonDays.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ElectiveLessonDay>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectiveLessonDays.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        _context.ExecuteFutureAction();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
        _context.ExecuteFutureAction();
    }

    public async Task<IEnumerable<ElectiveLessonDay>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ElectiveLessonDays.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}