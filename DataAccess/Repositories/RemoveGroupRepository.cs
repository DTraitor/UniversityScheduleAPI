using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class RemoveGroupRepository : IRemoveGroupRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<RemoveGroupRepository> _logger;

    public RemoveGroupRepository(ScheduleDbContext scheduleDbContext, ILogger<RemoveGroupRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public Task<List<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void AddRange(IEnumerable<Group> toRemove)
    {
        throw new NotImplementedException();
    }

    public void RemoveRange(IEnumerable<Group> toRemove)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}