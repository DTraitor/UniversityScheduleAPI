using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveChangeHandler : IChangeHandler<ElectiveLesson>
{
    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<ElectiveChangeHandler> _logger;

    public ElectiveChangeHandler(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<ElectiveChangeHandler> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ElectiveLesson>> HandleChanges(IEnumerable<ElectiveLesson> oldLessons, ICollection<ElectiveLesson> newLessons, CancellationToken token)
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

        foreach (var existingLesson in existingLessons)
        {
            newLessons.Remove(existingLesson);
        }

        return existingLessons;
    }
}