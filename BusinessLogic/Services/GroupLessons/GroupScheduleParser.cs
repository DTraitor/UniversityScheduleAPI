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

        var wrappers = document.DocumentNode.SelectNodes("//div[@class='wrapper']");

        if (wrappers == null)
        {
            throw new InvalidOperationException("No schedules found in group.");
        }

        foreach (var wrapper in wrappers)
        {
            var header = wrapper.SelectSingleNode(".//h2");
            bool weekNumber = header.InnerText.Contains("2");

            var schedules = wrapper.SelectNodes(".//table[@class='schedule']/tbody/tr");

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
                    List<string> teachers = [];// Todo: handle https://portal.nau.edu.ua/schedule/group?id=4688

                    if (teacherNodes != null)
                    {
                        teachers = teacherNodes
                        .Select(n => n.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim())
                        .ToList();
                    }

                    scheduleLessons.Add(new GroupLesson
                    {
                        Title = name.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Type = activity?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Location = room?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        Teacher = teachers,

                        StartTime = StartTimes[j],
                        Length = TimeSpan.FromMinutes(95),

                        Week = weekNumber,
                        DayOfWeek = k == 7 ? DayOfWeek.Sunday : (DayOfWeek)k+1,
                    });
                }
            }
        }

        return scheduleLessons;
    }
}