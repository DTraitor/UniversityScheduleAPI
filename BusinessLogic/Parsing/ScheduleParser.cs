using BusinessLogic.Helpers;
using BusinessLogic.Parsing.Interfaces;
using DataAccess.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Parsing;

public class ScheduleParser : IScheduleParser
{
    private readonly ILogger<ScheduleParser> _logger;

    public ScheduleParser(ILogger<ScheduleParser> logger)
    {
        _logger = logger;
    }

    public bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash)
    {
        var weeks = document.DocumentNode.SelectNodes("//table[@class='schedule']");
        newHash = Hashing.ComputeHash(String.Join(' ', weeks.Select(w => w.InnerHtml)));

        return newHash != oldHash;
    }

    public ICollection<LessonEntry> ParseSchedule(HtmlDocument document)
    {
        var scheduleLessons = new List<LessonEntry>();

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
                var timeNode = schedules[j].SelectSingleNode(".//th[@class='hour-name']/div[@class='full-name']");
                var parsed = timeNode.InnerText.Split('-');

                var beginTime = TimeSpan.Parse(parsed[0]);
                var duration = TimeSpan.Parse(parsed[1]) - beginTime;

                var pair = schedules[j].SelectNodes(".//td/div[@class='pairs']");

                for (int k = 0; k < pair.Count; k++)
                {
                    var lessonsNodes = pair[k].SelectNodes(".//div[@class='pair  vertical']");
                    if (lessonsNodes == null)
                        continue;

                    foreach (var lesson in lessonsNodes)
                    {
                        string name = lesson.SelectSingleNode(".//div[@class='subject']")?.InnerText;
                        string? activity = lesson.SelectSingleNode(".//div[@class='activity-tag']")?.InnerText;
                        string? room = lesson.SelectSingleNode(".//div[@class='room']")?.InnerText;

                        int subgroupNumber = -1;

                        var subgroupNode = lesson.SelectSingleNode(".//div[@class='subgroup']");
                        if (subgroupNode != null)
                        {
                            subgroupNumber = int.Parse(subgroupNode.InnerText.Replace("Підгрупа ", string.Empty));
                        }
                        else
                        {
                            subgroupNode = lesson.SelectSingleNode(".//div[@class='activity-tag']/span");

                            if (subgroupNode != null)
                            {
                                subgroupNumber = int.Parse(subgroupNode.InnerText);
                            }
                        }

                        var teacherNodes = lesson.SelectNodes(".//div[@class='teacher']/a");
                        List<string> teachers = [];

                        if (teacherNodes != null)
                        {
                            teachers = teacherNodes
                            .Select(n => n.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim())
                            .ToList();
                        }

                        scheduleLessons.Add(new LessonEntry
                        {
                            Title = name.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                            Type = activity?.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                            Location = room?.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("ауд. ", string.Empty).Trim(),
                            Teachers = teachers,
                            SubGroupNumber = subgroupNumber,

                            StartTime = beginTime,
                            Length = duration,

                            Week = weekNumber,
                            DayOfWeek = k == 7 ? DayOfWeek.Sunday : (DayOfWeek)k+1,
                        });
                    }
                }
            }
        }

        return scheduleLessons;
    }
}