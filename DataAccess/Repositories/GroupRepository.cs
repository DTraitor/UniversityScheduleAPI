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

    public async Task<Group?> GetByIdAsync(int id)
    {
        return await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Groups.ToListAsync(cancellationToken);
    }

    public void Add(Group entity)
    {
        _context.Groups.Add(entity);
    }

    public void Update(Group entity)
    {
        _context.Groups.Update(entity);
    }

    public void Delete(Group entity)
    {
        _context.Groups.Remove(entity);
    }

    public void AddRange(IEnumerable<Group> entities)
    {
        _context.Groups.AddRange(entities);
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

    public int SaveChanges()
    {
        return _context.SaveChanges();
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

    public async Task<IEnumerable<string>> GetFacultyNamesAsync()
    {
        return await _context.Groups.GroupBy(g => g.FacultyName)
            .Select(x => x.Key)
            .ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetBachelorGroupsAsync(string facultyName)
    {
        return await _context.Groups
            .Where(g => g.FacultyName == facultyName)
            .Where(g => g.GroupName[0] == 'Б')
            .ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetMasterGroupsAsync(string facultyName)
    {
        return await _context.Groups
            .Where(g => g.FacultyName == facultyName)
            .Where(g => g.GroupName[0] != 'Б')
            .ToListAsync();
    }

    public async Task<Group?> GetByNameAsync(string groupName)
    {
        return await _context.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
    }
}