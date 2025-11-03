using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class SelectedLessonEntryRepository : ISelectedLessonEntryRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<SelectedLessonEntryRepository> _logger;

    public SelectedLessonEntryRepository(ScheduleDbContext scheduleDbContext, ILogger<SelectedLessonEntryRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(SelectedLessonEntry entity)
    {
        _context.SelectedLessonEntries.Add(entity);
    }

    public void Update(SelectedLessonEntry entity)
    {
        _context.SelectedLessonEntries.Update(entity);
    }

    public void Delete(SelectedLessonEntry entity)
    {
        _context.SelectedLessonEntries.Remove(entity);
    }

    public void AddRange(IEnumerable<SelectedLessonEntry> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<SelectedLessonEntry> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<SelectedLessonEntry> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<SelectedLessonEntry?> GetByIdAsync(int id)
    {
        return await _context.SelectedLessonEntries.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<SelectedLessonEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedLessonEntries.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        using var transaction = _context.Database.BeginTransaction();

        _context.ExecuteFutureAction();
        _context.SaveChanges();

        transaction.Commit();
    }

    public async Task<IEnumerable<SelectedLessonEntry>> GetBySourceIds(IEnumerable<int> sourceIds)
    {
        return await _context.SelectedLessonEntries.Where(x => sourceIds.Contains(x.SourceId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonEntry>> GetByUserIds(IEnumerable<int> userIds)
    {
        return await _context.SelectedLessonEntries.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonEntry>> GetByUserId(int userId)
    {
        return await _context.SelectedLessonEntries.Where(x => x.UserId == userId).ToListAsync();
    }
}