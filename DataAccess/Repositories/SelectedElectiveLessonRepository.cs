using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class SelectedElectiveLessonRepository : ISelectedElectiveLesson
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<SelectedElectiveLessonRepository> _logger;

    public SelectedElectiveLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<SelectedElectiveLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Add(entity);
    }

    public void Update(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Update(entity);
    }

    public void Delete(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Remove(entity);
    }

    public void AddRange(IEnumerable<SelectedElectiveLesson> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<SelectedElectiveLesson> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<SelectedElectiveLesson> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<SelectedElectiveLesson?> GetByIdAsync(int id)
    {
        return await _context.SelectedElectiveLessonEntries.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<SelectedElectiveLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedElectiveLessonEntries.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IEnumerable<SelectedElectiveLesson>> GetBySourceIds(IEnumerable<int> sourceIds)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => sourceIds.Contains(x.LessonSourceId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedElectiveLesson>> GetByUserIds(IEnumerable<int> userIds)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedElectiveLesson>> GetByUserId(int userId)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => x.UserId == userId).ToListAsync();
    }
}