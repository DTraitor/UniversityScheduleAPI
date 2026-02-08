using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task AddRangeAsync(ICollection<UsageMetric> entities)
    {
        await _context.BulkInsertAsync(entities);
    }

    public async Task UpdateRangeAsync(ICollection<UsageMetric> entities)
    {
        await _context.BulkUpdateAsync(entities);
    }

    public async Task RemoveRangeAsync(ICollection<UsageMetric> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<UsageMetric?> GetByIdAsync(int id)
    {
        return await _context.UsageMetrics.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<UsageMetric>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UsageMetrics.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}