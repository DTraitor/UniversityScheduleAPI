using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupRepository : IRepository<Group>
{
    Task AddRangeAsync(IEnumerable<Group> toUpdate);
    void AddOrUpdate(Group group);
    void Remove(Group group);
    Task<IEnumerable<string>> GetFacultyNamesAsync();
    Task<IEnumerable<Group>> GetBachelorGroupsAsync(string facultyName);
    Task<IEnumerable<Group>> GetMasterGroupsAsync(string facultyName);
    Task<Group?> GetByNameAsync(string groupName);
}