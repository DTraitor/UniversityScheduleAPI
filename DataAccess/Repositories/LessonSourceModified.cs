using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
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

    public void AddRange(IEnumerable<LessonSourceModified> toAdd)
    {
        _context.FutureAction(x => x.BulkInsert(toAdd));
    }

    public void UpdateRange(IEnumerable<LessonSourceModified> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<LessonSourceModified> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<LessonSourceModified?> GetByIdAsync(int id)
    {
        return await _context.LessonSourceModifications.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<LessonSourceModified>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LessonSourceModifications.ToListAsync(cancellationToken);
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
}