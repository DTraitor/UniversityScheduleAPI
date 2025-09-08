using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        IGroupRepository groupRepository,
        ILogger<GroupService> logger)
    {
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<bool> GroupExists(string groupName)
    {
        return (await _groupRepository.GetByNameAsync(groupName)) != null;
    }

    public async Task<IEnumerable<string>> GetFacultiesAsync()
    {
        return await _groupRepository.GetFacultyNamesAsync();
    }

    public async Task<IEnumerable<GroupDto>> GetGroupByDegreeAsync(string facultyName, bool bachelor)
    {
        IEnumerable<Group> result;
        if (bachelor)
            result = await _groupRepository.GetBachelorGroupsAsync(facultyName);
        else
            result = await _groupRepository.GetMasterGroupsAsync(facultyName);

        return result.Select(x => new GroupDto
        {
            Id = x.Id,
            FacultyName = x.FacultyName,
            GroupName = x.GroupName,
        });
    }
}