using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EFCore.BulkExtensions;

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

    public async Task AddRangeAsync(ICollection<LessonEntry> entities)
    {
        await _context.BulkInsertAsync(entities);
    }

    public async Task UpdateRangeAsync(ICollection<LessonEntry> entities)
    {
        await _context.BulkUpdateAsync(entities);
    }

    public async Task RemoveRangeAsync(ICollection<LessonEntry> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<LessonEntry?> GetByIdAsync(int id)
    {
        return await _context.LessonEntries.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<LessonEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonEntries.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<LessonEntry>> GetBySourceIdAsync(int sourceId)
    {
        return await _context.LessonEntries.Where(x => x.SourceId == sourceId).ToListAsync();
    }

    public async Task<ICollection<LessonEntry>> GetBySourceIdsAsync(ICollection<int> sourceIds)
    {
        return await _context.LessonEntries.Where(x => sourceIds.Contains(x.SourceId)).ToListAsync();
    }
}