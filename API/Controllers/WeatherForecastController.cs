using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Reader.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IGroupScheduleReader _reader;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IGroupScheduleReader reader)
    {
        _logger = logger;
        _reader = reader;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using var httpClient = new HttpClient();
        var html = httpClient.GetStringAsync("https://portal.nau.edu.ua/schedule/group?id=352").Result;

        // 2. Load into HtmlAgilityPack
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var result = _reader.ReadLessons(doc, 1);

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}