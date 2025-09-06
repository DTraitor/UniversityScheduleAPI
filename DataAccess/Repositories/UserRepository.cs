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

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}