using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class LessonSourceModifiedRepositoryRepository : ILessonSourceModifiedRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<LessonSourceModifiedRepositoryRepository> _logger;

    public LessonSourceModifiedRepositoryRepository(ScheduleDbContext scheduleDbContext, ILogger<LessonSourceModifiedRepositoryRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(LessonSourceModified entity)
    {
        _context.LessonSourceModifications.Add(entity);
    }

    public void Update(LessonSourceModified entity)
    {
        _context.LessonSourceModifications.Update(entity);
    }

    public void Delete(LessonSourceModified entity)
    {
        _context.LessonSourceModifications.Remove(entity);
    }

    public async Task AddRangeAsync(ICollection<LessonSourceModified> toAdd)
    {
        await _context.BulkInsertAsync(toAdd);
    }

    public async Task UpdateRangeAsync(ICollection<LessonSourceModified> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<LessonSourceModified> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<LessonSourceModified?> GetByIdAsync(int id)
    {
        return await _context.LessonSourceModifications.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<LessonSourceModified>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonSourceModifications.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}