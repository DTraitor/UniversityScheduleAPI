using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<GroupRepository> _logger;

    public GroupRepository(ScheduleDbContext scheduleDbContext, ILogger<GroupRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public async Task<List<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Groups.ToListAsync(cancellationToken);
    }

    public void RemoveRange(IEnumerable<Group> toRemove)
    {
        _context.Groups.RemoveRange(toRemove);
    }

    public void Update(IEnumerable<Group> toUpdate)
    {
        _context.Groups.UpdateRange(toUpdate);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}