using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class SelectedLessonSourceRepository : ISelectedLessonSourceRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<SelectedLessonSourceRepository> _logger;

    public SelectedLessonSourceRepository(ScheduleDbContext scheduleDbContext, ILogger<SelectedLessonSourceRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(SelectedLessonSource entity)
    {
        _context.SelectedLessonSources.Add(entity);
    }

    public void Update(SelectedLessonSource entity)
    {
        _context.SelectedLessonSources.Update(entity);
    }

    public void Delete(SelectedLessonSource entity)
    {
        _context.SelectedLessonSources.Remove(entity);
    }

    public void AddRange(IEnumerable<SelectedLessonSource> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<SelectedLessonSource> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<SelectedLessonSource> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<SelectedLessonSource?> GetByIdAsync(int id)
    {
        return await _context.SelectedLessonSources.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<SelectedLessonSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedLessonSources.ToListAsync(cancellationToken);
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

    public async Task<IEnumerable<SelectedLessonSource>> GetByUserId(int userId)
    {
        return await _context.SelectedLessonSources.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonSource>> GetByUserIds(IEnumerable<int> userIds)
    {
        return await _context.SelectedLessonSources.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonSource>> GetBySourceIds(IEnumerable<int> sourceIds)
    {
        return await _context.SelectedLessonSources.Where(x => sourceIds.Contains(x.SourceId)).ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonSource>> GetByUserIdAndSourceType(int userId, LessonSourceType lessonSourceType)
    {
        return await _context.SelectedLessonSources
            .Where(x => x.UserId == userId && x.LessonSourceType == lessonSourceType)
            .ToListAsync();
    }

    public async Task<IEnumerable<SelectedLessonSource>> GetByUserIdsAndSourceType(IEnumerable<int> userIds, LessonSourceType lessonSourceType)
    {
        return await _context.SelectedLessonSources
            .Where(x => userIds.Contains(x.UserId) && x.LessonSourceType == lessonSourceType)
            .ToListAsync();
    }
}