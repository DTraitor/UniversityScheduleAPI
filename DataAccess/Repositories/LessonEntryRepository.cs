using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class LessonEntryRepository : ILessonEntryRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<LessonEntryRepository> _logger;

    public LessonEntryRepository(ScheduleDbContext scheduleDbContext, ILogger<LessonEntryRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(LessonEntry entity)
    {
        _context.LessonEntries.Add(entity);
    }

    public void Update(LessonEntry entity)
    {
        _context.LessonEntries.Update(entity);
    }

    public void Delete(LessonEntry entity)
    {
        _context.LessonEntries.Remove(entity);
    }

    public void AddRange(IEnumerable<LessonEntry> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<LessonEntry> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<LessonEntry> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<LessonEntry?> GetByIdAsync(int id)
    {
        return await _context.LessonEntries.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<LessonEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonEntries.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        _context.ExecuteFutureAction();
        _context.SaveChanges();
    }
}