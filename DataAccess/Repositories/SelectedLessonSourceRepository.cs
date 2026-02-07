using Common.Enums;
using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
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

    public async Task AddRangeAsync(ICollection<SelectedLessonSource> toAdd)
    {
        await _context.BulkInsertAsync(toAdd);
    }

    public async Task UpdateRangeAsync(ICollection<SelectedLessonSource> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<SelectedLessonSource> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<SelectedLessonSource?> GetByIdAsync(int id)
    {
        return await _context.SelectedLessonSources.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<SelectedLessonSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedLessonSources.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<SelectedLessonSource>> GetByUserId(int userId)
    {
        return await _context.SelectedLessonSources.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<ICollection<SelectedLessonSource>> GetByUserIds(ICollection<int> userIds)
    {
        return await _context.SelectedLessonSources.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<ICollection<SelectedLessonSource>> GetBySourceIds(ICollection<int> sourceIds)
    {
        return await _context.SelectedLessonSources.Where(x => sourceIds.Contains(x.SourceId)).ToListAsync();
    }

    public async Task<ICollection<SelectedLessonSource>> GetByUserIdAndSourceType(int userId, LessonSourceType lessonSourceType)
    {
        return await _context.SelectedLessonSources
            .Where(x => x.UserId == userId && x.LessonSourceType == lessonSourceType)
            .ToListAsync();
    }

    public async Task<ICollection<SelectedLessonSource>> GetByUserIdsAndSourceType(ICollection<int> userIds, LessonSourceType lessonSourceType)
    {
        return await _context.SelectedLessonSources
            .Where(x => userIds.Contains(x.UserId) && x.LessonSourceType == lessonSourceType)
            .ToListAsync();
    }
}