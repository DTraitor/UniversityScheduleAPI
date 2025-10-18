using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.GroupLessons;

public class GroupLessonUserUpdaterService : IUserLessonUpdaterService<GroupLesson>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupLessonRepository  _groupLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<ScheduleParsingOptions> _options;
    private readonly ILogger<GroupLessonUserUpdaterService> _logger;

    public GroupLessonUserUpdaterService(
        IGroupRepository groupRepository,
        IGroupLessonRepository groupLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ScheduleParsingOptions> options,
        ILogger<GroupLessonUserUpdaterService> logger)
    {
        _groupRepository = groupRepository;
        _groupLessonRepository = groupLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _logger = logger;
    }

    public async Task ProcessModifiedUser(IEnumerable<UserModified> modifiedUsers)
    {
        var users = await _userRepository.GetByIdsAsync(modifiedUsers.Select(u => u.UserId));
        var removed = _userLessonRepository.RemoveByUserIdsAndLessonSourceType(users.Select(x => x.Id), LessonSourceTypeEnum.Group);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        users = users.Where(x => x.GroupId != null).ToList();

        var groups = await _groupRepository.GetByIdsAsync(users.Select(x => x.GroupId.Value));

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);
        var lessons = await _groupLessonRepository.GetByGroupIdsAsync(groups.Select(x => x.Id));

        foreach (var user in users)
        {
            _userLessonRepository.AddRange(
                ScheduleLessonsMapper.Map(lessons.Where(x => x.GroupId == user.Id), _options.Value.StartTime, _options.Value.EndTime, timeZone)
                    .Select(x =>
                    {
                        x.UserId = user.Id;
                        return x;
                    }));
        }

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }

    public ProcessedByEnum ProcessedBy => ProcessedByEnum.GroupLessons;
}