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

    public async Task AddRangeAsync(IEnumerable<Group> toUpdate)
    {
        await _context.Groups.AddRangeAsync(toUpdate);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public void AddOrUpdate(Group group)
    {
        var existing = _context.Groups.Find(group.Id);

        if (existing == null)
        {
            _context.Groups.Add(group);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(group);
        }
    }

    public void Remove(Group group)
    {
        _context.Groups.Remove(group);
    }

    public async Task<Group?> GetByNameAsync(string groupName)
    {
        return await _context.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
    }
}