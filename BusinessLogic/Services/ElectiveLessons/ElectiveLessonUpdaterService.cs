using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveLessonUpdaterService : ILessonUpdaterService<ElectiveLesson, ElectiveLessonModified>
{
    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-09-01T00:00:00.000000+03:00");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-11-30T00:00:00.000000+03:00");

    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<ElectiveLessonUpdaterService> _logger;

    public ElectiveLessonUpdaterService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<ElectiveLessonUpdaterService> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
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

        foreach (var user in users)
        {
            var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceType(user.Id, LessonSourceTypeEnum.Elective);
            user.ElectedLessonIds = user.ElectedLessonIds.Except(removedElected.Select(x => x.Id)).ToList();
            _userLessonOccurenceRepository.ClearByLessonIds(removed);
        }

        var electiveDay = await _dayRepository.GetByIdAsync(modifiedEntry.Key);

        foreach (var user in users.Where(x => x.ElectedLessonIds.Count != 0))
        {
            _userLessonRepository.AddRange(
                ElectiveLessonsMapper.Map(
                        electiveLessons.Where(x => user.ElectedLessonIds.Contains(x.Id)),
                        electiveDay,
                        BEGIN_UNIVERSITY_DATE,
                        END_UNIVERSITY_DATE)
                    .Select(x =>
                    {
                        x.UserId = user.Id;
                        return x;
                    }));
        }

        await _userRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _electedRepository.SaveChangesAsync();
    }
}