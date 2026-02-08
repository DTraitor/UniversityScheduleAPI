using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<ICollection<UserModified>> GetNotProcessedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserModifications.Take(100).ToListAsync(cancellationToken);
    }

    public void Add(int userId)
    {
        _context.Add(new UserModified { UserId = userId });
    }

    public async Task RemoveProcessedAsync(ICollection<UserModified> toRemove)
    {
        await _context.BulkDeleteAsync(toRemove);
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