using System.Collections.Concurrent;
using BusinessLogic.Configuration;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveScheduleReader : IScheduleReader<ElectiveLesson, ElectiveLessonModified>
{
    private const string ScheduleLocation = "/schedule/pairs/elective?day_id={0}&hour_id={1}";

    private readonly List<string> _electivesAsGroups = [
        "https://portal.nau.edu.ua/schedule/group?id=4807",
    ];

    private readonly List<TimeSpan> _startTimes =
    [
        TimeSpan.Parse("8:00"),
        TimeSpan.Parse("9:50"),
        TimeSpan.Parse("11:40"),
        TimeSpan.Parse("13:30"),
        TimeSpan.Parse("15:20"),
        TimeSpan.Parse("17:10"),
        TimeSpan.Parse("19:00")
    ];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IScheduleParser<ElectiveLesson> _lessonParser;
    private readonly IScheduleParser<GroupLesson> _groupParser;
    private readonly IRepository<ElectiveLessonDay> _electiveLessonDayRepository;
    private readonly IOptions<ElectiveScheduleParsingOptions> _options;
    private readonly ILogger<ElectiveScheduleReader> _logger;

    public ElectiveScheduleReader(
        IHttpClientFactory httpClientFactory,
        IScheduleParser<ElectiveLesson> lessonParser,
        IScheduleParser<GroupLesson> groupParser,
        IRepository<ElectiveLessonDay> electiveLessonDayRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        ILogger<ElectiveScheduleReader> logger)
    {
        _options = options;
        _httpClientFactory = httpClientFactory;
        _lessonParser = lessonParser;
        _groupParser = groupParser;
        _electiveLessonDayRepository = electiveLessonDayRepository;
        _logger = logger;
    }

    public async Task<(IEnumerable<ElectiveLessonModified>, ICollection<ElectiveLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);

        var currentElectiveDays = await _electiveLessonDayRepository.GetAllAsync(cancellationToken);
        if (!currentElectiveDays.Any())
        {
            List<ElectiveLessonDay> toProcess = new();

            for (int i = 1; i <= 14; i++)
            {
                for (int j = 1; j <= 7; j++)
                {
                    toProcess.Add(new ElectiveLessonDay
                    {
                        DayId = i,
                        HourId = j,
                    });
                }
            }

            currentElectiveDays = toProcess;
            _electiveLessonDayRepository.AddRange(toProcess);

            await _electiveLessonDayRepository.SaveChangesAsync(cancellationToken);
        }

        ConcurrentBag<ElectiveLesson> parsedLessons = new ConcurrentBag<ElectiveLesson>();
        ConcurrentBag<ElectiveLessonModified> lessonModifications = new ConcurrentBag<ElectiveLessonModified>();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken
        };

        var electives = Parallel.ForEachAsync(currentElectiveDays, options, async (elective, ct) =>
        {
            var lessons = await FetchElectiveScheduleAsync(string.Format(ScheduleLocation, elective.DayId, elective.HourId), elective, ct);
            if (lessons == null)
                return;

            foreach (var electiveLesson in lessons)
            {
                parsedLessons.Add(electiveLesson);
            }

            lessonModifications.Add(new ElectiveLessonModified
            {
                ElectiveDayId = elective.Id,
            });
        });

        var electivesAsGroup = Parallel.ForEachAsync(_electivesAsGroups, options, async (group, ct) =>
        {
            var lessons = await FetchElectiveScheduleFromGroupAsync(group, ct);
            foreach (var electiveLesson in lessons)
            {
                var dayId = (int)electiveLesson.DayOfWeek;

                if (dayId == 0)
                    dayId = 7;

                if(electiveLesson.Week)
                    dayId += 7;

                var electiveDay = currentElectiveDays
                    .Where(x => x.DayId == dayId)
                    .FirstOrDefault(x => x.HourId == (_startTimes.IndexOf(electiveLesson.StartTime) + 1));

                if (electiveDay == null)
                {
                    _logger.LogError("Couldn't find elective day of the lesson in elective group HREF: {Href}", group);
                    return;
                }

                parsedLessons.Add(new()
                {
                    ElectiveLessonDayId = electiveDay.Id,
                    Title = electiveLesson.Title,
                    Type = electiveLesson.Type,
                    Location = electiveLesson.Location,
                    Teacher = electiveLesson.Teacher,
                    StartTime = electiveLesson.StartTime,
                    Length = electiveLesson.Length,
                });

                lessonModifications.Add(new()
                {
                    ElectiveDayId = electiveDay.Id,
                });
            }
        });

        await Task.WhenAll(electives, electivesAsGroup);

        await _electiveLessonDayRepository.SaveChangesAsync(cancellationToken);

        return (lessonModifications, parsedLessons.ToList());
    }

    private async Task<IEnumerable<ElectiveLesson>?> FetchElectiveScheduleAsync(string href, ElectiveLessonDay day, CancellationToken stoppingToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);
        var scheduleString = await httpClient.GetStringAsync(href, stoppingToken);

        var scheduleDoc = new HtmlDocument();
        scheduleDoc.LoadHtml(scheduleString);

        return _lessonParser.ReadLessons(scheduleDoc).Select(x =>
        {
            x.ElectiveLessonDayId = day.Id;
            return x;
        });
    }

    private async Task<IEnumerable<GroupLesson>?> FetchElectiveScheduleFromGroupAsync(string href, CancellationToken stoppingToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);
            var scheduleString = await httpClient.GetStringAsync(href, stoppingToken);

            var scheduleDoc = new HtmlDocument();
            scheduleDoc.LoadHtml(scheduleString);

            return _groupParser.ReadLessons(scheduleDoc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered when processing elective group HREF: {Href}", href);
            throw;
        }
    }
}