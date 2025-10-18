using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveLessonUpdaterService : ILessonUpdaterService<ElectiveLesson, ElectiveLessonModified>
{
    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<ElectiveScheduleParsingOptions> _options;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<ElectiveLessonUpdaterService> _logger;

    public ElectiveLessonUpdaterService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        UserAlertService userAlertService,
        ILogger<ElectiveLessonUpdaterService> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(IEnumerable<ElectiveLessonModified> modifiedEntry)
    {
        List<int> dayIds = modifiedEntry.Select(x => x.Key).ToList();

        var electedLessons = await _electedRepository.GetByElectiveDayIdsAsync(dayIds);
        var users = await _userRepository.GetByIdsAsync(electedLessons.Select(x => x.UserId));
        if(!users.Any())
            return;

        var electiveLessons = await _lessonRepository.GetByElectiveDayIdsAsync(dayIds);
        HashSet<int> electiveLessonIds = new HashSet<int>(electiveLessons.Select(x => x.Id));
        var removedElected = electedLessons.Where(x => !electiveLessonIds.Contains(x.ElectiveLessonId)).ToList();

        _electedRepository.RemoveRange(removedElected);

        var electiveDays = await _dayRepository.GetByIdsAsync(dayIds);

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        var removed = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(
            users.Select(x => x.Id), LessonSourceTypeEnum.Elective, dayIds);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        foreach (var removedLesson in removedElected)
        {
            var electiveDay = electiveDays.FirstOrDefault(x => x.Id == removedLesson.Id);
            _userAlertService.CreateUserAlert(removedLesson.UserId, UserAlertType.ElectiveLessonRemoved, new()
            {
                { "LessonName", removedLesson.Name },
                { "LessonType", removedLesson.Type ?? "" },
                { "LessonDay", electiveDay.DayId.ToString() },
                { "LessonStartTime", electiveDay.HourId.ToString() },
            });
        }

        foreach (var electiveLesson in electiveLessons.GroupBy(x => x.ElectiveLessonDayId))
        {
            var electiveDay = electiveDays.FirstOrDefault(x => x.Id == electiveLesson.Key);
            var dayLessons = electiveLesson
                .Where(x => electedLessons.Select(y => y.ElectiveLessonId).Contains(x.Id));

            foreach (var user in users)
            {
                _userLessonRepository.AddRange(
                    ElectiveLessonsMapper.Map(
                            dayLessons.Where(
                                x => electedLessons
                                    .Where(x => x.UserId == user.Id)
                                    .Select(x => x.ElectiveLessonId)
                                    .Contains(x.Id)),
                            electiveDay,
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

        await _userLessonRepository.SaveChangesAsync();
        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _electedRepository.SaveChangesAsync();
    }
}