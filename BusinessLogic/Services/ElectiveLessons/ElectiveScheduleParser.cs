using System.Text.RegularExpressions;
using BusinessLogic.Helpers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveScheduleParser : IScheduleParser<ElectiveLesson>
{
    public bool HasHashChanged(HtmlDocument document, string oldHash, out string newHash)
    {
        var schedule = document.DocumentNode.SelectSingleNode("//table[@class='schedule']");
        newHash = Hashing.ComputeHash(schedule.InnerHtml);

        return newHash != oldHash;
    }

    public IEnumerable<ElectiveLesson> ReadLessons(HtmlDocument document)
    {
        var dateTime = document.DocumentNode.SelectNodes("//h2")[1].InnerText;
        var matches = Regex.Match(dateTime, @"^.* (\d\d?:\d\d)-(\d\d?:\d\d)$");
        var startTime = TimeOnly.Parse(matches.Groups[1].Value);
        var endTime = TimeOnly.Parse(matches.Groups[2].Value);
        var length = endTime - startTime;

        List<ElectiveLesson>  electiveLessons = new List<ElectiveLesson>();

        var lessons = document.DocumentNode.SelectNodes("//table[@class='schedule']/tbody/tr");

        if (lessons == null)
            return [];

        foreach (var lesson in lessons)
        {
            var values = lesson.SelectNodes(".//td");

            var teacherNodes = lesson.SelectNodes(".//div/a");
            List<string> teachers = [];

            if (teacherNodes != null)
            {
                teachers = teacherNodes
                    .Select(n => n.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim())
                    .ToList();
            }

            electiveLessons.Add(new ElectiveLesson
            {
                Title = values[0].InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                Type = values[1].InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                Location = values[3].InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                Teacher = teachers,

                StartTime = startTime,
                Length = length,
            });
        }

        return electiveLessons;
    }
}