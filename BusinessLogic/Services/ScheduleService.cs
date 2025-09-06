using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<ScheduleService> logger)
    {
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<LessonDto>> GetScheduleForDate(DateTimeOffset dateTime, int userId)
    {
        var occurrences = await _userLessonOccurenceRepository.GetByUserIdAsync(userId);
        var lessons = await _userLessonRepository.GetByIdsAsync(occurrences.Select(x => x.LessonId));

        return lessons.Select(l => new LessonDto
        {
            Title = l.Title,
            LessonType = l.LessonType,
            Teacher = l.Teacher,
            Location = l.Location,
            Cancelled = l.Cancelled,
            BeginTime = l.BeginTime,
            Duration = l.Duration,
        });
    }
}