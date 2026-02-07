using BusinessLogic.Configuration;
using BusinessLogic.Parsing.Interfaces;
using Common.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Parsing;

public class ScheduleReader : IScheduleReader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IScheduleParser _lessonParser;
    private readonly IOptions<ScheduleParsingOptions> _options;
    private readonly ILogger<ScheduleReader> _logger;

    public ScheduleReader(
        IHttpClientFactory httpClientFactory,
        IScheduleParser lessonParser,
        IOptions<ScheduleParsingOptions> options,
        ILogger<ScheduleReader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _lessonParser = lessonParser;
        _options = options;
        _logger = logger;
    }

    public async Task<ICollection<LessonEntry>?> ReadSchedule(LessonSource source, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.Value.ScheduleUrl);
        var scheduleString = await httpClient.GetStringAsync($"schedule/group?id={source.HrefId}", cancellationToken);

        var scheduleDoc = new HtmlDocument();
        scheduleDoc.LoadHtml(scheduleString);

        if (!_lessonParser.HasHashChanged(scheduleDoc, source.PageHash, out var newHash))
            return null;
        source.PageHash = newHash;

        return _lessonParser.ParseSchedule(scheduleDoc).Select(x =>
        {
            x.SourceId = source.Id;
            return x;
        }).ToArray();
    }
}