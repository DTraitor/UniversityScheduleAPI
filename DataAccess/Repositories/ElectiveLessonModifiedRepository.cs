using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ElectiveLessonModifiedRepository : IRepository<ElectiveLessonModified>
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ElectiveLessonModifiedRepository> _logger;

    public ElectiveLessonModifiedRepository(ScheduleDbContext scheduleDbContext, ILogger<ElectiveLessonModifiedRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(ElectiveLessonModified entity)
    {
        _context.ElectiveLessonModifications.Add(entity);
    }

    public void Update(ElectiveLessonModified entity)
    {
        _context.ElectiveLessonModifications.Update(entity);
    }

    public void Delete(ElectiveLessonModified entity)
    {
        _context.ElectiveLessonModifications.Remove(entity);
    }

    public void AddRange(IEnumerable<ElectiveLessonModified> entities)
    {
        _context.ElectiveLessonModifications.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<ElectiveLessonModified> entity)
    {
        _context.ElectiveLessonModifications.UpdateRange(entity);
    }

    public void RemoveRange(IEnumerable<ElectiveLessonModified> entities)
    {
        _context.ElectiveLessonModifications.RemoveRange(entities);
    }

    public async Task<ElectiveLessonModified?> GetByIdAsync(int id)
    {
        return await _context.ElectiveLessonModifications.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ElectiveLessonModified>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectiveLessonModifications.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }
}