using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.GroupLessons;

public class GroupLessonUpdaterService : ILessonUpdaterService<GroupLesson, GroupLessonModified>
{
    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-09-01T00:00:00.000000+03:00");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-11-30T00:00:00.000000+03:00");

    private readonly IGroupRepository _groupRepository;
    private readonly IGroupLessonRepository  _groupLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<GroupLessonUpdaterService> _logger;

    public GroupLessonUpdaterService(
        IGroupRepository groupRepository,
        IGroupLessonRepository groupLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<GroupLessonUpdaterService> logger)
    {
        _groupRepository = groupRepository;
        _groupLessonRepository = groupLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(GroupLessonModified modifiedEntry)
    {
        var users = await _userRepository.GetByGroupIdAsync(modifiedEntry.Key);
        if(!users.Any())
            return;

        foreach (var user in users)
        {
            var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceType(user.Id, LessonSourceTypeEnum.Group);
            _userLessonOccurenceRepository.ClearByLessonIds(removed);
        }

        var group = await _groupRepository.GetByIdAsync(modifiedEntry.Key);
        if (group == null)
        {
            foreach (var user in users)
            {
                user.GroupId = null;
                _userRepository.Update(user);
            }
            //TODO: Alert users
            await _userRepository.SaveChangesAsync();
            return;
        }

        var lessons = await _groupLessonRepository.GetByGroupIdAsync(group.Id);
        foreach (var user in users)
        {
            await _userLessonRepository.AddRangeAsync(
                ScheduleLessonsMapper.Map(lessons, BEGIN_UNIVERSITY_DATE, END_UNIVERSITY_DATE)
                    .Select(x =>
                    {
                        x.UserId = user.Id;
                        return x;
                    }));
        }

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }
}