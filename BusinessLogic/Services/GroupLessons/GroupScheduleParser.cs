using BusinessLogic.Helpers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.GroupLessons;

public class GroupScheduleParser : IScheduleParser<GroupLesson>
{
    private readonly ILogger<GroupScheduleParser> _logger;

    public GroupScheduleParser(ILogger<GroupScheduleParser> logger)
    {
        _logger = logger;
    }

    private readonly List<TimeOnly> StartTimes = new()
    {
        TimeOnly.Parse("8:00"),
        TimeOnly.Parse("9:50"),
        TimeOnly.Parse("11:40"),
        TimeOnly.Parse("13:30"),
        TimeOnly.Parse("15:20"),
        TimeOnly.Parse("17:10"),
        TimeOnly.Parse("19:00"),
    };

    public bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash)
    {
        var weeks = document.DocumentNode.SelectNodes("//table[@class='schedule']");
        newHash = Hashing.ComputeHash(String.Join(' ', weeks.Select(w => w.InnerHtml)));

        return newHash != oldHash;
    }

    public IEnumerable<GroupLesson> ReadLessons(HtmlDocument document)
    {
        var scheduleLessons = new List<GroupLesson>();

        var weeks = document.DocumentNode.SelectNodes("//table[@class='schedule']");

        if (weeks == null)
        {
            throw new InvalidOperationException("No tables with class 'schedule' found in group.");
        }

        for (int i = 0; i < 2; i++)
        {
            var schedules = weeks[i].SelectNodes(".//tbody/tr");

            for(int j = 0; j < schedules.Count(); j++)
            {
                var pair = schedules[j].SelectNodes(".//td/div[@class='pairs']");

                for (int k = 0; k < pair.Count; k++)
                {
                    var lesson = pair[k].SelectSingleNode(".//div[@class='pair  vertical']");
                    if (lesson == null)
                        continue;

                    string name = lesson.SelectSingleNode(".//div[@class='subject']")?.InnerText;
                    string? activity = lesson.SelectSingleNode(".//div[@class='activity-tag']")?.InnerText;
                    string? room = lesson.SelectSingleNode(".//div[@class='room']")?.InnerText;
                    var teacherNodes = lesson.SelectNodes(".//div[@class='teacher']/a");
                    var teachers = teacherNodes
                        .Select(n => n.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim())
                        .ToList();

                    scheduleLessons.Add(new GroupLesson
                    {
                        Title = name.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Type = activity?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Location = room?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Teacher = teachers,

                        StartTime = StartTimes[k],
                        Length = TimeSpan.FromMinutes(95),

                        Week = i == 1,
                        DayOfWeek = (DayOfWeek)j,
                    });
                }
            }
        }

        return scheduleLessons;
    }
}