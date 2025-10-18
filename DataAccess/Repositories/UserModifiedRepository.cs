using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserModifiedRepository : IUserModifiedRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserModifiedRepository> _logger;

    public UserModifiedRepository(ScheduleDbContext scheduleDbContext, ILogger<UserModifiedRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<UserModified>> GetNotProcessed(ProcessedByEnum flag, CancellationToken cancellationToken = default)
    {
        return await _context.UserModifications.Where(x => x.ToProcess == flag).Take(100).ToListAsync(cancellationToken);
    }

    public void Add(int userId, ProcessedByEnum toProcessBy)
    {
        _context.Add(new UserModified { UserId = userId, ToProcess = toProcessBy });
    }

    public void AddProcessByAll(int userId)
    {
        foreach (ProcessedByEnum value in Enum.GetValues(typeof(ProcessedByEnum)))
        {
            _context.Add(new UserModified { UserId = userId, ToProcess = value });
        }
    }

    public void RemoveProcessed(IEnumerable<UserModified> toRemove)
    {
        _context.UserModifications.RemoveRange(toRemove);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ExecuteFutureAction();
        return await _context.SaveChangesAsync(cancellationToken);
    }
}