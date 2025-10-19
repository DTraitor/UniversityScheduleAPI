using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services;

public class LessonUpdaterService : ILessonUpdaterService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository  _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedLessonEntryRepository _selectedLessonEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<ElectiveScheduleParsingOptions> _options;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<LessonUpdaterService> _logger;

    public LessonUpdaterService(
        ILessonSourceRepository lessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedLessonEntryRepository selectedLessonEntryRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        IUserAlertService userAlertService,
        ILogger<LessonUpdaterService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _selectedLessonEntryRepository = selectedLessonEntryRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(IEnumerable<LessonSourceModified> modifiedEntry)
    {
        HashSet<int> modifiedSourceIds = modifiedEntry.Select(x => x.SourceId).ToHashSet();
        var selectedSources = await _selectedLessonSourceRepository.GetBySourceIds(modifiedSourceIds);
        var selectedEntries = await _selectedLessonEntryRepository.GetBySourceIds(modifiedSourceIds);

        var sourceIds = selectedSources
            .Select(x => x.SourceId)
            .Union(selectedEntries.Select(x => x.SourceId))
            .ToHashSet();

        var users = await _userRepository.GetByIdsAsync(
            selectedSources
                .Select(x => x.UserId)
                .Union(selectedEntries.Select(x => x.UserId))
            );

        if(!users.Any())
            return;

        var sources = await _lessonSourceRepository.GetByIdsAsync(sourceIds);
        var entries = await _lessonEntryRepository.GetBySourceIdsAsync(sourceIds);

        foreach (var user in users)
        {

        }

        var electiveLessons = await _lessonRepository.GetByElectiveDayIdsAsync(dayIds);
        HashSet<int> electiveLessonIds = new HashSet<int>(electiveLessons.Select(x => x.Id));
        var removedElected = electedLessons.Where(x => !electiveLessonIds.Contains(x.ElectiveLessonId)).ToList();

        _electedRepository.RemoveRange(removedElected);

        var electiveDays = await _dayRepository.GetByIdsAsync(dayIds);

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        var removed = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(
            users.Select(x => x.Id), SelectedLessonSourceType.Elective, dayIds);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        foreach (var removedLesson in removedElected)
        {
            var electiveDay = electiveDays.FirstOrDefault(x => x.Id == removedLesson.ElectiveLessonDayId);
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
    }
}