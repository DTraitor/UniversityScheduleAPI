using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupRepository
{
    Task<List<Group>> GetAllAsync(CancellationToken cancellationToken);
    void RemoveRange(IEnumerable<Group> toRemove);
    Task AddRangeAsync(IEnumerable<Group> toUpdate);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void AddOrUpdate(Group group);
    void Remove(Group group);
    Task<IEnumerable<string>> GetFacultyNamesAsync();
    Task<IEnumerable<Group>> GetBachelorGroupsAsync(string facultyName);
    Task<IEnumerable<Group>> GetMasterGroupsAsync(string facultyName);
    Task<Group?> GetByNameAsync(string groupName);
}