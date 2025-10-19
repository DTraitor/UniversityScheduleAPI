using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class GroupService : IGroupService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        ILessonSourceRepository lessonSourceRepository,
        ILogger<GroupService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _logger = logger;
    }

    public async Task<bool> GroupExists(string groupName)
    {
        return (await _lessonSourceRepository.GetByNameAndSourceTypeAsync(groupName, LessonSourceType.Group)) != null;
    }
}