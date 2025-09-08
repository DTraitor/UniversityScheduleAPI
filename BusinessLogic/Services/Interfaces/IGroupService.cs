using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<bool> GroupExists(string groupName);
    Task<IEnumerable<string>> GetFacultiesAsync();
    Task<IEnumerable<GroupDto>> GetGroupByDegreeAsync(string facultyName, bool bachelor);
}