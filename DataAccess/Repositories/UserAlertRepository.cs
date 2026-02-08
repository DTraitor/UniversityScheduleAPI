using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserAlertRepository : IUserAlertRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserAlertRepository> _logger;

    public UserAlertRepository(ScheduleDbContext scheduleDbContext, ILogger<UserAlertRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(UserAlert entity)
    {
        _context.UserAlerts.Add(entity);
    }

    public void Update(UserAlert entity)
    {
        _context.UserAlerts.Update(entity);
    }

    public void Delete(UserAlert entity)
    {
        _context.UserAlerts.Remove(entity);
    }

    public async Task AddRangeAsync(ICollection<UserAlert> entities)
    {
        await _context.BulkInsertAsync(entities);
    }

    public async Task UpdateRangeAsync(ICollection<UserAlert> entities)
    {
        await _context.BulkUpdateAsync(entities);
    }

    public async Task RemoveRangeAsync(ICollection<UserAlert> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<UserAlert?> GetByIdAsync(int id)
    {
        return await _context.UserAlerts.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<UserAlert>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserAlerts.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<UserAlert>> GetAllLimitAsync(int batchSize)
    {
        return await _context.UserAlerts.OrderBy(x => x.Id).Take(batchSize).ToListAsync();
    }

    public async Task RemoveByIdsAsync(ICollection<int> alerts)
    {
        var toRemove = _context.UserAlerts.Where(x => alerts.Contains(x.Id)).AsEnumerable();
        await _context.BulkDeleteAsync(toRemove);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}