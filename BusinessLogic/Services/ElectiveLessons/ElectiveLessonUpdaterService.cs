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
    private readonly ILogger<ElectiveLessonUpdaterService> _logger;

    public ElectiveLessonUpdaterService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        ILogger<ElectiveLessonUpdaterService> logger)
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

    public async Task ProcessModifiedEntry(ElectiveLessonModified modifiedEntry)
    {
        var electedLessons = await _electedRepository.GetByElectiveDayIdAsync(modifiedEntry.Key);
        var users = await _userRepository.GetByIdsAsync(electedLessons.Select(x => x.UserId));
        if(!users.Any())
            return;

        var electiveLessons = await _lessonRepository.GetByElectiveDayIdAsync(modifiedEntry.Key);
        HashSet<int> electiveLessonIds = new HashSet<int>(electiveLessons.Select(x => x.Id));
        var removedElected = electedLessons.Where(x => !electiveLessonIds.Contains(x.ElectiveLessonId));

        _electedRepository.RemoveRange(removedElected);

        var electiveDay = await _dayRepository.GetByIdAsync(modifiedEntry.Key);

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZone);

        foreach (var user in users)
        {
            var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceTypeAndLessonSourceId(
                user.Id, LessonSourceTypeEnum.Elective, modifiedEntry.Key);
            _userLessonOccurenceRepository.ClearByLessonIds(removed);

            var userElectedLessons = await _electedRepository.GetByUserId(user.Id);
            if(!userElectedLessons.Any())
                return;

            _userLessonRepository.AddRange(
                ElectiveLessonsMapper.Map(
                        electiveLessons.Where(x => userElectedLessons.Select(x => x.ElectiveLessonId).Contains(x.Id)),
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

        await _userLessonRepository.SaveChangesAsync();
        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _electedRepository.SaveChangesAsync();
    }
}