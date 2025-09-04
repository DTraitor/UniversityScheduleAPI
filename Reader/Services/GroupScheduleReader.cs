using DataAccess.Models;
using HtmlAgilityPack;
using Reader.Services.Interfaces;

namespace Reader.Services;

public class GroupScheduleReader : IGroupScheduleReader
{
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

    //TODO: Parallel all of this
    public IEnumerable<ScheduleLesson> ReadLessons(HtmlDocument document, int groupId, CancellationToken stoppingToken)
    {
        var scheduleLessons = new List<ScheduleLesson>();

        var weeks = document.DocumentNode.SelectNodes("//table[@class='schedule']");

        if (weeks == null)
        {
            //TODO: log it
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
                    string activity =  lesson.SelectSingleNode(".//div[@class='activity-tag']")?.InnerText;
                    string room =  lesson.SelectSingleNode(".//div[@class='room']")?.InnerText;
                    string teacher =  lesson.SelectSingleNode(".//div[@class='teacher']/a")?.InnerText;

                    scheduleLessons.Add(new ScheduleLesson
                    {
                        GroupId = groupId,
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