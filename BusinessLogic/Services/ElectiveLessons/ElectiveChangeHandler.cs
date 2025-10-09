using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveChangeHandler : IChangeHandler<ElectiveLesson>
{
    private readonly ILogger<ElectiveChangeHandler> _logger;

    public ElectiveChangeHandler(ILogger<ElectiveChangeHandler> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<ElectiveLesson>> HandleChanges(IEnumerable<ElectiveLesson> oldLessons, ICollection<ElectiveLesson> newLessons, CancellationToken token)
    {
        Dictionary<int, Dictionary<string, Dictionary<string, ElectiveLesson>>> oldLessonsDictionary = new();

        foreach (var electiveLesson in oldLessons)
        {
            if (!oldLessonsDictionary.TryGetValue(electiveLesson.ElectiveLessonDayId, out var dayDict))
            {
                oldLessonsDictionary[electiveLesson.ElectiveLessonDayId] = dayDict = new();
            }

            if(!dayDict.TryGetValue(electiveLesson.Title, out var dictionary))
            {
                dayDict[electiveLesson.Title] = dictionary = new Dictionary<string, ElectiveLesson>();
            }

            dictionary[electiveLesson.Type ?? ""] = electiveLesson;
        }

        var existingLessons = new List<ElectiveLesson>();

        foreach (var electiveLesson in newLessons)
        {
            if (oldLessonsDictionary.TryGetValue(electiveLesson.ElectiveLessonDayId, out var dayDict))
            {
                if (dayDict.TryGetValue(electiveLesson.Title, out var dictionary))
                {
                    if (dictionary.TryGetValue(electiveLesson.Type ?? "", out var lesson))
                    {
                        electiveLesson.Id = lesson.Id;
                        lesson.Length = electiveLesson.Length;
                        lesson.Location = electiveLesson.Location;
                        lesson.Teacher = electiveLesson.Teacher;
                        lesson.StartTime = electiveLesson.StartTime;
                        existingLessons.Add(lesson);
                    }
                }
            }
        }

        var idsToRemove = existingLessons.Select(x => x.Id).ToHashSet();
        foreach (var existingLesson in newLessons.Where(x => idsToRemove.Contains(x.Id)).ToList())
        {
            newLessons.Remove(existingLesson);
        }

        return existingLessons;
    }
}