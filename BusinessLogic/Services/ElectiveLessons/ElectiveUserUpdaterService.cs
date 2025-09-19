using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveUserUpdaterService : IUserLessonUpdaterService<ElectiveLesson>
{
    private readonly DateTimeOffset BEGIN_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-09-01T00:00:00.000000+03:00");
    private readonly DateTimeOffset END_UNIVERSITY_DATE = DateTimeOffset.Parse("2025-11-30T00:00:00.000000+03:00");

    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<ElectiveUserUpdaterService> _logger;

    public ElectiveUserUpdaterService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<ElectiveUserUpdaterService> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
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

        var removed = _userLessonRepository.RemoveByUserIdAndLessonSourceType(user.Id, LessonSourceTypeEnum.Elective);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        var electedLessons = await _electedRepository.GetByUserId(user.Id);
        if (!electedLessons.Any())
            return;

        var electiveLessons = await _lessonRepository.GetByIdsAsync(electedLessons.Select(x => x.ElectiveLessonId));
        var electiveDays = await _dayRepository.GetByIdsAsync(electedLessons.Select(x => x.ElectiveLessonDayId));

        foreach (var lessonsGroup in electiveLessons.GroupBy(x => x.ElectiveLessonDayId))
        {
            _userLessonRepository.AddRange(
                ElectiveLessonsMapper.Map(
                        lessonsGroup,
                        electiveDays.FirstOrDefault(x => x.Id == lessonsGroup.Key),
                        BEGIN_UNIVERSITY_DATE,
                        END_UNIVERSITY_DATE)
                    .Select(x =>
                    {
                        x.UserId = user.Id;
                        return x;
                    }));
        }

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }

    public ProcessedByEnum ProcessedBy => ProcessedByEnum.ElectiveLessons;
}