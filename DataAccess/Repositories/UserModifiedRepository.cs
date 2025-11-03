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

    public async Task<IEnumerable<UserModified>> GetNotProcessed(CancellationToken cancellationToken = default)
    {
        return await _context.UserModifications.Take(100).ToListAsync(cancellationToken);
    }

    public void Add(int userId)
    {
        _context.Add(new UserModified { UserId = userId });
    }

    public void RemoveProcessed(IEnumerable<UserModified> toRemove)
    {
        _context.FutureAction(x => x.BulkDelete(toRemove));
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}