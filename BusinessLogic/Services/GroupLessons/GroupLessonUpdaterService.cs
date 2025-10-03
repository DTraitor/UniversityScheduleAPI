using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.GroupLessons;

public class GroupLessonUpdaterService : ILessonUpdaterService<GroupLesson, GroupLessonModified>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupLessonRepository  _groupLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<GroupScheduleParsingOptions> _options;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<GroupLessonUpdaterService> _logger;

    public GroupLessonUpdaterService(
        IGroupRepository groupRepository,
        IGroupLessonRepository groupLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<GroupScheduleParsingOptions> options,
        IUserAlertService userAlertService,
        ILogger<GroupLessonUpdaterService> logger)
    {
        _groupRepository = groupRepository;
        _groupLessonRepository = groupLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(GroupLessonModified modifiedEntry)
    {
        var users = await _userRepository.GetByGroupIdAsync(modifiedEntry.Key);
        if(!users.Any())
            return;

        foreach (var user in users)
        {
            var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceTypeAndLessonSourceId(
                user.Id, LessonSourceTypeEnum.Group, modifiedEntry.Key);
            _userLessonOccurenceRepository.ClearByLessonIds(removed);
        }

        var group = await _groupRepository.GetByIdAsync(modifiedEntry.Key);
        if (group == null)
        {
            foreach (var user in users)
            {
                user.GroupId = null;
                _userRepository.Update(user);

                await _userAlertService.CreateUserAlert(user.Id, UserAlertType.GroupRemoved, new()
                {
                    {"GroupName", group.GroupName},
                    {"FacultyName", group.FacultyName},
                });
            }
        }

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        var lessons = await _groupLessonRepository.GetByGroupIdAsync(group.Id);
        foreach (var user in users)
        {
            _userLessonRepository.AddRange(
                ScheduleLessonsMapper.Map(lessons, _options.Value.StartTime, _options.Value.EndTime, timeZone)
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