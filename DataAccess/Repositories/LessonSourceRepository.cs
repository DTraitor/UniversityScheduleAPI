using Common.Enums;
using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task AddRangeAsync(ICollection<LessonSource> toAdd)
    {
        await _context.BulkInsertAsync(toAdd);
    }

    public async Task UpdateRangeAsync(ICollection<LessonSource> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<LessonSource> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<LessonSource?> GetByIdAsync(int id)
    {
        return await _context.LessonSources.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<LessonSource>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonSources.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<LessonSource?> GetByNameAndSourceTypeAsync(string name, LessonSourceType lessonSourceType)
    {
        return await _context.LessonSources.FirstOrDefaultAsync(x => x.SourceType == lessonSourceType && x.Name == name);
    }

    public async Task<ICollection<LessonSource>> GetBySourceTypeAsync(LessonSourceType lessonSourceType)
    {
        return await _context.LessonSources.Where(x => x.SourceType == lessonSourceType).ToListAsync();
    }

    public async Task<ICollection<LessonSource>> GetByNameAndLimitAsync(string name, int limit)
    {
        return await _context.LessonSources.Where(x => x.Name.ToLower().Contains(name)).Take(limit).ToListAsync();
    }

    public async Task<ICollection<LessonSource>> GetByIdsAsync(ICollection<int> ids)
    {
        return await _context.LessonSources.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}