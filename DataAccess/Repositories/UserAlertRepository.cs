using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
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

    public void AddRange(IEnumerable<UserAlert> entities)
    {
        _context.UserAlerts.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<UserAlert> entities)
    {
        _context.UserAlerts.UpdateRange(entities);
    }

    public void RemoveRange(IEnumerable<UserAlert> entities)
    {
        _context.UserAlerts.RemoveRange(entities);
    }

    public async Task<UserAlert?> GetByIdAsync(int id)
    {
        return await _context.UserAlerts.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<UserAlert>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserAlerts.ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ExecuteFutureAction();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        _context.ExecuteFutureAction();
        return _context.SaveChanges();
    }

    public async Task<IEnumerable<UserAlert>> GetAllLimitAsync(int batchSize)
    {
        return await _context.UserAlerts.OrderBy(x => x.Id).Take(batchSize).ToListAsync();
    }

    public void RemoveByIds(IEnumerable<int> alerts)
    {
        _context.UserAlerts.RemoveRange(_context.UserAlerts.Where(x => alerts.Contains(x.Id)));
    }
}