using System.Collections.Concurrent;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveScheduleReader : IScheduleReader<ElectiveLesson, ElectiveLessonModified>
{
    private const string ScheduleApi = "https://portal.nau.edu.ua/";
    private const string ScheduleLocation = "/schedule/pairs/elective?day_id={0}&hour_id={1}";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IScheduleParser<ElectiveLesson> _lessonParser;
    private readonly IRepository<ElectiveLessonDay> _electiveLessonDayRepository;
    private readonly ILogger<ElectiveScheduleReader> _logger;

    public ElectiveScheduleReader(
        IHttpClientFactory httpClientFactory,
        IScheduleParser<ElectiveLesson> lessonParser,
        IRepository<ElectiveLessonDay> electiveLessonDayRepository,
        ILogger<ElectiveScheduleReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _lessonParser = lessonParser;
        _electiveLessonDayRepository = electiveLessonDayRepository;
        _logger = logger;
    }

    public async Task<(IEnumerable<ElectiveLessonModified>, IEnumerable<ElectiveLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(ScheduleApi);

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

        await Parallel.ForEachAsync(currentElectiveDays, cancellationToken, async (elective, ct) =>
        {
            var lessons = await FetchGroupScheduleAsync(string.Format(ScheduleLocation, elective.DayId, elective.HourId), elective, ct);
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

        await _electiveLessonDayRepository.SaveChangesAsync(cancellationToken);

        return (lessonModifications, parsedLessons);
    }

    private async Task<IEnumerable<ElectiveLesson>?> FetchGroupScheduleAsync(string href, ElectiveLessonDay day, CancellationToken stoppingToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(ScheduleApi);
            var scheduleString = await httpClient.GetStringAsync(href, stoppingToken);

            var scheduleDoc = new HtmlDocument();
            scheduleDoc.LoadHtml(scheduleString);

            if (!_lessonParser.HasHashChanged(scheduleDoc, day.HashPage, out var newHash))
                return null;
            day.HashPage = newHash;

            return _lessonParser.ReadLessons(scheduleDoc).Select(x =>
            {
                x.ElectiveLessonDayId = day.Id;
                return x;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encountered when processing group HREF: {Href}", href);
            throw;
        }
    }
}