using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IUsageMetricRepository _usageMetricRepository;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IUsageMetricRepository usageMetricRepository,
        ILogger<ScheduleService> logger)
    {
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _usageMetricRepository = usageMetricRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<LessonDto>> GetScheduleForDate(DateTimeOffset dateTime, long userTelegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(userTelegramId);
        if(user == null)
            throw new KeyNotFoundException("User not found");

        DateTimeOffset dayBegin = dateTime.Date.ToUniversalTime();
        var occurrences = await _userLessonOccurenceRepository.GetByUserIdAndBetweenDateAsync(user.Id, dayBegin, dayBegin.AddDays(1));
        var lessons = await _userLessonRepository.GetByIdsAsync(occurrences.Select(x => x.LessonId));

        try
        {
            _usageMetricRepository.Add(new UsageMetric() { Timestamp = DateTimeOffset.UtcNow, UserId = user.Id });
            await _usageMetricRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when adding a new usage metric");
        }

        return lessons.Select(l => new LessonDto
        {
            Title = l.Title,
            LessonType = l.LessonType,
            Teacher = l.Teacher,
            Location = l.Location,
            Cancelled = l.Cancelled,
            BeginTime = l.BeginTime,
            Duration = l.Duration,
            TimeZoneId = l.TimeZoneId,
        });
    }
}