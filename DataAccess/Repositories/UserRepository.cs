using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ScheduleDbContext userDbContext, ILogger<UserRepository> logger)
    {
        _context = userDbContext;
        _logger = logger;
    }

    public void Add(User entity)
    {
        _context.Users.Add(entity);
    }

    public void Update(User entity)
    {
        _context.Users.Update(entity);
    }

    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
    }

    public async Task AddRangeAsync(ICollection<User> entities)
    {
        await _context.BulkInsertAsync(entities);
    }

    public async Task UpdateRangeAsync(ICollection<User> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<User> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<ICollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public async Task<ICollection<User>> GetByIdsAsync(ICollection<int> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return _context.Users.Where(u => u.TelegramId == telegramId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}