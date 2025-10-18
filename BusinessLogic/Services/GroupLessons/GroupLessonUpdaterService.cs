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

    public async Task ProcessModifiedEntry(IEnumerable<GroupLessonModified> modifiedEntries)
    {
        var groupsIds = modifiedEntries.Select(x => x.Key).ToArray();
        var users = await _userRepository.GetByGroupIdsAsync(modifiedEntries.Select(x => x.Key));
        if(!users.Any())
            return;

        var removed = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(
            users.Select(x => x.Id), LessonSourceTypeEnum.Group, groupsIds);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        var groups = await _groupRepository.GetByIdsAsync(groupsIds);
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        foreach (var user in users.Where(x => groups.All(g => g.Id != x.Id)))
        {
            user.GroupId = null;
            _userRepository.Update(user);

            _userAlertService.CreateUserAlert(user.Id, UserAlertType.GroupRemoved, new()
            {
                {"GroupName", user.GroupName},
            });
        }

        var lessons = await _groupLessonRepository.GetByGroupIdsAsync(groupsIds);


        foreach (var user in users)
        {
            _userLessonRepository.AddRange(
                ScheduleLessonsMapper.Map(lessons.Where(x => x.GroupId == user.GroupId), _options.Value.StartTime, _options.Value.EndTime, timeZone)
                    .Select(x =>
                    {
                        x.UserId = user.Id;
                        return x;
                    }));
        }

        await _userRepository.SaveChangesAsync();
        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }
}