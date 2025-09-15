using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.GroupLessons;

public class GroupLessonUserUpdaterService : IUserLessonUpdaterService<GroupLesson>
{
    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-09-01T00:00:00.000000+03:00");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-11-30T00:00:00.000000+03:00");

    private readonly IGroupRepository _groupRepository;
    private readonly IGroupLessonRepository  _groupLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<GroupLessonUserUpdaterService> _logger;

    public GroupLessonUserUpdaterService(
        IGroupRepository groupRepository,
        IGroupLessonRepository groupLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<GroupLessonUserUpdaterService> logger)
    {
        _groupRepository = groupRepository;
        _groupLessonRepository = groupLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task ProcessModifiedUser(UserModified modifiedUser)
    {
        var user = await _userRepository.GetByIdAsync(modifiedUser.Key);

        if (user == null)
        {
            _logger.LogError($"User {modifiedUser.Key} doesn't exist.");
            return;
        }

        var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceType(user.Id, LessonSourceTypeEnum.Group);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        if (user.GroupId == null)
        {
            await _userLessonOccurenceRepository.SaveChangesAsync();
            await _userLessonRepository.SaveChangesAsync();
            return;
        }

        var group = await _groupRepository.GetByIdAsync(user.GroupId.Value);
        if (group == null)
            return;

        var lessons = await _groupLessonRepository.GetByGroupIdAsync(group.Id);

        _userLessonRepository.AddRange(
            ScheduleLessonsMapper.Map(lessons, BEGIN_UNIVERSITY_DATE, END_UNIVERSITY_DATE)
            .Select(x =>
            {
                x.UserId = user.Id;
                return x;
            }));

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }

    public ProcessedByEnum ProcessedBy => ProcessedByEnum.GroupLessons;
}