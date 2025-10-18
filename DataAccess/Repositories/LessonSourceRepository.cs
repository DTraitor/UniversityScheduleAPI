using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class LessonSourceRepository : ILessonSourceRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<LessonSourceRepository> _logger;

    public LessonSourceRepository(ScheduleDbContext scheduleDbContext, ILogger<LessonSourceRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(LessonSource entity)
    {
        _context.LessonSources.Add(entity);
    }

    public void Update(LessonSource entity)
    {
        _context.LessonSources.Update(entity);
    }

    public void Delete(LessonSource entity)
    {
        _context.LessonSources.Remove(entity);
    }

    public void AddRange(IEnumerable<LessonSource> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<LessonSource> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<LessonSource> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<LessonSource?> GetByIdAsync(int id)
    {
        return await _context.LessonSources.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<LessonSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonSources.ToListAsync(cancellationToken);
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