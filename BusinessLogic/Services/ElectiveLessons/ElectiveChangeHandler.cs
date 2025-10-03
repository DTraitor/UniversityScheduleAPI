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

    public async Task HandleChanges(IEnumerable<ElectiveLesson> oldLessons, ICollection<ElectiveLesson> newLessons, CancellationToken token)
    {
        Dictionary<int, Dictionary<string, Dictionary<string, int>>> oldLessonsDictionary = new();

        foreach (var electiveLesson in oldLessons)
        {
            if (!oldLessonsDictionary.TryGetValue(electiveLesson.ElectiveLessonDayId, out var dayDict))
            {
                oldLessonsDictionary[electiveLesson.ElectiveLessonDayId] = dayDict = new();
            }

            if(!dayDict.TryGetValue(electiveLesson.Title, out var dictionary))
            {
                dayDict[electiveLesson.Title] = dictionary = new Dictionary<string, int>();
            }

            dictionary[electiveLesson.Type ?? ""] = electiveLesson.Id;
        }

        var existingLessons = new List<ElectiveLesson>();

        foreach (var electiveLesson in newLessons)
        {
            if (oldLessonsDictionary.TryGetValue(electiveLesson.ElectiveLessonDayId, out var dayDict))
            {
                if (dayDict.TryGetValue(electiveLesson.Title, out var dictionary))
                {
                    if (dictionary.TryGetValue(electiveLesson.Type ?? "", out var id))
                    {
                        electiveLesson.Id = id;
                        existingLessons.Add(electiveLesson);
                    }
                }
            }
        }
    }
}