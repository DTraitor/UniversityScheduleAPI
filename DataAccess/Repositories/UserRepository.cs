using DataAccess.Domain;
using DataAccess.Enums;
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

    public void Add(User entity)
    {
        _context.Users.Add(entity);
    }

    void IRepository<User>.Update(User entity)
    {
        _context.Users.Update(entity);
    }

    public void Delete(User entity)
    {
        _context.Users.Remove(entity);
    }

    public void AddRange(IEnumerable<User> entities)
    {
        _context.Users.AddRange(entities);
    }

    public void UpdateRange(IEnumerable<User> entity)
    {
        _context.Users.UpdateRange(entity);
    }

    public void RemoveRange(IEnumerable<User> entities)
    {
        _context.Users.RemoveRange(entities);
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.ToListAsync(cancellationToken);
    }

    public User Update(User user)
    {
        return _context.Users.Update(user).Entity;
    }

    public async Task<IEnumerable<User>> GetByGroupIdsAsync(IEnumerable<int> groupIds, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => u.GroupId.HasValue && groupIds.Contains(u.GroupId.Value)).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => u.GroupId.HasValue && u.GroupId.Value == groupId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return (await _context.Users.AddAsync(user, cancellationToken)).Entity;
    }

    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return _context.Users.Where(u => u.TelegramId == telegramId).FirstOrDefaultAsync(cancellationToken);
    }
}