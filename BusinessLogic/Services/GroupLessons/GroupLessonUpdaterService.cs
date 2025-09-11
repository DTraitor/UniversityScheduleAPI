using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.GroupLessons;

public class GroupLessonUpdaterService : ILessonUpdaterService<GroupLesson, GroupLessonModified>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupLessonRepository  _groupLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly ILogger<GroupLessonUpdaterService> _logger;

    public GroupLessonUpdaterService(
        IGroupRepository groupRepository,
        IGroupLessonRepository groupLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        ILogger<GroupLessonUpdaterService> logger)
    {
        _groupRepository = groupRepository;
        _groupLessonRepository = groupLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(GroupLessonModified modifiedEntry)
    {
        var group = await _groupRepository.GetById(modifiedEntry.Key);
        if (group == null)
        {
            var users = await _userRepository.GetByGroupIdAsync(modifiedEntry.Key);
        }
    }
}