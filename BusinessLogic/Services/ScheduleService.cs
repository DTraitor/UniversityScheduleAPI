using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models.Internal;
using Common.Result;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class ScheduleService : IScheduleService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IUsageMetricService _usageMetricService;
    private readonly ILogger<ScheduleService> _logger;

    private readonly DateTimeOffset ScheduleLimitStart = DateTimeOffset.Parse("2026-02-02T00:00:00+02:00");
    private readonly DateTimeOffset ScheduleLimitEnd = DateTimeOffset.Parse("2026-06-30T23:59:59+03:00");

    public ScheduleService(
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IUsageMetricService usageMetricService,
        ILogger<ScheduleService> logger)
    {
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _usageMetricService = usageMetricService;
        _logger = logger;
    }

    public async Task<Result<ICollection<UserLesson>, (DateTimeOffset, DateTimeOffset)>> GetScheduleForDate(DateTimeOffset dateTime, long userTelegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(userTelegramId);
        if (user == null)
            return ErrorType.UserNotFound;

        DateTimeOffset dayBegin = dateTime.Date.ToUniversalTime();

        if (dayBegin < ScheduleLimitStart || dayBegin > ScheduleLimitEnd)
            return (ErrorType.TimetableDateOutOfRange, (dayBegin, ScheduleLimitEnd));

        var occurrences = await _userLessonOccurenceRepository.GetByUserIdAndBetweenDateAsync(user.Id, dayBegin, dayBegin.AddDays(1));
        var lessons = await _userLessonRepository.GetByIdsAsync(occurrences.Select(x => x.LessonId).ToArray());

        _usageMetricService.AddUsage(DateTimeOffset.UtcNow, dayBegin, user.Id);

        return new Result<ICollection<UserLesson>, (DateTimeOffset, DateTimeOffset)>(lessons);
    }
}