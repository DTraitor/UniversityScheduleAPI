using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(UserDbContext userDbContext, ILogger<UserRepository> logger)
    {
        _context = userDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetByGroupIdsAsync(IEnumerable<int> groupIds)
    {
        return await _context.Users.Where(u => u.GroupId.HasValue && groupIds.Contains(u.GroupId.Value)).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds)
    {
        return await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
    }

    public User Update(User user)
    {
        return _context.Users.Update(user).Entity;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return (await _context.Users.AddAsync(user, cancellationToken)).Entity;
    }

    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return _context.Users.Where(u => u.TelegramId == telegramId).FirstOrDefaultAsync();
    }
}