using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class ElectiveService : IElectiveService
{
    private readonly List<TimeSpan> StartTimes = new()
    {
        TimeSpan.Parse("0:00"),
        TimeSpan.Parse("8:00"),
        TimeSpan.Parse("9:50"),
        TimeSpan.Parse("11:40"),
        TimeSpan.Parse("13:30"),
        TimeSpan.Parse("15:20"),
        TimeSpan.Parse("17:10"),
        TimeSpan.Parse("19:00"),
    };

    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectiveLessonDayRepository _electiveDayRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserModifiedRepository _userModifiedRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ElectiveService> _logger;

    public ElectiveService(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IElectiveLessonDayRepository electiveDayRepository,
        IUserModifiedRepository userModifiedRepository,
        IUserRepository userRepository,
        ILogger<ElectiveService> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userModifiedRepository = userModifiedRepository;
        _electiveDayRepository = electiveDayRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ElectiveLessonDayDto>> GetPossibleDays()
    {
        var uniqueLessonDays = await _lessonRepository.GetUniqueLessonDaysAsync();
        var allDays = await _dayRepository.GetByIdsAsync(uniqueLessonDays);

        return allDays.Select(x => new ElectiveLessonDayDto
        {
            Id = x.Id,
            WeekNumber = x.DayId / 7,
            DayOfWeek = (DayOfWeek)(x.DayId % 7),
            StartTime = StartTimes[x.HourId],
        });
    }

    public async Task<IEnumerable<ElectiveLessonDto>> GetElectiveLessons(int electiveDayId, string partialLessonName)
    {
        var lessons = await _lessonRepository.GetByDayIdAndPartialNameAsync(electiveDayId, partialLessonName);
        var days = await _dayRepository.GetByIdsAsync(lessons.Select(x => x.ElectiveLessonDayId));

        return lessons
            .Join(days, x => x.ElectiveLessonDayId, y => y.Id, (x, y) => new ElectiveLessonDto
            {
                Id = x.Id,
                Title = x.Title,
                Type = x.Type,
                Location = x.Location,
                Teacher = x.Teacher,
                WeekNumber = y.DayId / 7,
                DayOfWeek = (DayOfWeek)(y.DayId % 7),
                StartTime = x.StartTime,
                Length = x.Length,
            });
    }

    public async Task<IEnumerable<ElectiveLessonDto>> GetCurrentLessons(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var electiveDays = await _electiveDayRepository.GetAllAsync();
        var elected = await _electedRepository.GetByUserId(user.Id);
        return (await _lessonRepository.GetByIdsAsync(elected.Select(x => x.ElectiveLessonId)))
            .Join(elected, x => x.Id, y => y.ElectiveLessonId, (x, y) =>
            {
                var electiveDay = electiveDays.FirstOrDefault(z => z.Id == x.ElectiveLessonDayId);
                return new ElectiveLessonDto
                {
                    Id = y.Id,
                    Title = x.Title,
                    Type = x.Type,
                    Location = x.Location,
                    Teacher = x.Teacher,
                    StartTime = x.StartTime,
                    Length = x.Length,
                    DayOfWeek = (DayOfWeek)((electiveDay.DayId %) - 1),
                    WeekNumber = electiveDay.DayId / 7,
                };
            });
    }

    public async Task CreateNewElectedLesson(CreateElectiveLessonDto newLessonDto)
    {
        var user = await _userRepository.GetByTelegramIdAsync(newLessonDto.TelegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var lesson = await _lessonRepository.GetByIdAsync(newLessonDto.ElectiveLessonId);
        if (lesson == null)
            throw new KeyNotFoundException("Lesson not found");

        ElectedLesson newLesson = new ElectedLesson
        {
            UserId = user.Id,
            Name = lesson.Title,
            Type = lesson.Type,
            ElectiveLessonDayId = lesson.ElectiveLessonDayId,
            ElectiveLessonId = lesson.Id,
        };

        _electedRepository.Add(newLesson);
        await _electedRepository.SaveChangesAsync();

        _userModifiedRepository.Add(user.Id, ProcessedByEnum.ElectiveLessons);
        await _userModifiedRepository.SaveChangesAsync();
    }

    public async Task RemoveElectedLesson(long telegramId, int lessonId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var lesson = await _electedRepository.GetByIdAsync(lessonId);
        if (lesson == null)
            return;

        if(lesson.UserId != user.Id)
            throw new InvalidOperationException("Lesson doesn't belong to the user");

        _electedRepository.Delete(lesson);
        await _electedRepository.SaveChangesAsync();

        _userModifiedRepository.Add(user.Id, ProcessedByEnum.ElectiveLessons);
        await _userModifiedRepository.SaveChangesAsync();
    }
}