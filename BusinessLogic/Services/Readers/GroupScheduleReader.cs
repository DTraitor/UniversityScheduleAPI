using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using BusinessLogic.Services.Readers.Interfaces;

namespace BusinessLogic.Services.Readers;

public class GroupScheduleReader : IGroupScheduleReader
{
    private readonly ILogger<GroupScheduleReader> _logger;

    public GroupScheduleReader(ILogger<GroupScheduleReader> logger)
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

    private string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    //TODO: Parallel all of this
    public IEnumerable<ScheduleLesson>? ReadLessons(HtmlDocument document, Group group, CancellationToken stoppingToken)
    {
        var scheduleLessons = new List<ScheduleLesson>();

        var weeks = document.DocumentNode.SelectNodes("//table[@class='schedule']");

        if (weeks == null)
        {
            throw new InvalidOperationException("No tables with class 'schedule' found in group.");
        }

        var newHash = ComputeHash(String.Join(' ', weeks.Select(w => w.InnerHtml)));
        if (group.SchedulePageHash == newHash)
        {
            return null;
        }
        group.SchedulePageHash = newHash;

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
                    string activity =  lesson.SelectSingleNode(".//div[@class='activity-tag']")?.InnerText;
                    string room =  lesson.SelectSingleNode(".//div[@class='room']")?.InnerText;
                    string teacher =  lesson.SelectSingleNode(".//div[@class='teacher']/a")?.InnerText;

                    scheduleLessons.Add(new ScheduleLesson
                    {
                        GroupId = group.Id,
                        Title = name,
                        Type = activity,
                        Location = room,
                        Teacher = teacher,

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