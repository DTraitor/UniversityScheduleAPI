using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveUserUpdaterService : IUserLessonUpdaterService<ElectiveLesson>
{
    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<ElectiveScheduleParsingOptions> _options;
    private readonly ILogger<ElectiveUserUpdaterService> _logger;

    public ElectiveUserUpdaterService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        ILogger<ElectiveUserUpdaterService> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _logger = logger;
    }

    public async Task ProcessModifiedUser(IEnumerable<UserModified> modifiedUsers)
    {
        var users = await _userRepository.GetByIdsAsync(modifiedUsers.Select(x => x.Key));

        var removed = _userLessonRepository.RemoveByUserIdsAndLessonSourceType(
            users.Select(x => x.Id),
            LessonSourceTypeEnum.Elective);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        var electedLessons = await _electedRepository.GetByUserIds(users.Select(x => x.Id));
        if (!electedLessons.Any())
            return;

        var electiveLessons = await _lessonRepository.GetByIdsAsync(electedLessons.Select(x => x.ElectiveLessonId));
        var electiveDays = await _dayRepository.GetByIdsAsync(electedLessons.Select(x => x.ElectiveLessonDayId));

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        foreach (var user in users)
        {
            foreach (var lessonsGroup in electiveLessons.Where(x => electedLessons
                         .Where(x => x.UserId == user.Id).Select(x => x.ElectiveLessonId).Contains(x.Id))
                         .GroupBy(x => x.ElectiveLessonDayId))
            {
                _userLessonRepository.AddRange(
                    ElectiveLessonsMapper.Map(
                            lessonsGroup,
                            electiveDays.FirstOrDefault(x => x.Id == lessonsGroup.Key),
                            _options.Value.StartTime.ToUniversalTime(),
                            _options.Value.EndTime.ToUniversalTime(),
                            timeZone)
                        .Select(x =>
                        {
                            x.UserId = user.Id;
                            return x;
                        }));
            }
        }

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }

    public ProcessedByEnum ProcessedBy => ProcessedByEnum.ElectiveLessons;
}