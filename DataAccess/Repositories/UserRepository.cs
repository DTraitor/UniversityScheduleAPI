using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
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
}