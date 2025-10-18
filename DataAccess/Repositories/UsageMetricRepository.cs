using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UsageMetricRepository : IUsageMetricRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UsageMetricRepository> _logger;

    public UsageMetricRepository(ScheduleDbContext scheduleDbContext, ILogger<UsageMetricRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(UsageMetric entity)
    {
        _context.UsageMetrics.Add(entity);
    }

    public void Update(UsageMetric entity)
    {
        _context.UsageMetrics.Update(entity);
    }

    public void Delete(UsageMetric entity)
    {
        _context.UsageMetrics.Remove(entity);
    }

    public void AddRange(IEnumerable<UsageMetric> entities)
    {
        _context.FutureAction(x => x.BulkInsert(entities));
    }

    public void UpdateRange(IEnumerable<UsageMetric> entities)
    {
        _context.FutureAction(x => x.BulkUpdate(entities));
    }

    public void RemoveRange(IEnumerable<UsageMetric> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<UsageMetric?> GetByIdAsync(int id)
    {
        return await _context.UsageMetrics.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<UsageMetric>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UsageMetrics.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        _context.ExecuteFutureAction();
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
        _context.ExecuteFutureAction();
    }
}