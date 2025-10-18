using BusinessLogic.Parsing.Interfaces;
using DataAccess.Models;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Parsing;

public class ChangeHandler : IChangeHandler
{
    private readonly ILogger<ChangeHandler> _logger;

    public ChangeHandler(ILogger<ChangeHandler> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<LessonEntry>> HandleChanges(IEnumerable<LessonEntry> oldLessons, ICollection<LessonEntry> newLessons, CancellationToken token)
    {
        var oldLessonsDict = oldLessons.ToDictionary(
            x => (x.SourceId, x.Week, x.DayOfWeek, x.StartTime, x.SubGroupNumber, x.Title, x.Type ?? string.Empty),
            x => x);


        var existingLessons = new List<LessonEntry>();

        foreach (var entry in newLessons)
        {
            if (oldLessonsDict.TryGetValue(
                    (entry.SourceId, entry.Week, entry.DayOfWeek, entry.StartTime, entry.SubGroupNumber, entry.Title, entry.Type ?? string.Empty),
                    out var oldEntry))
            {
                oldEntry.Location = entry.Location;
                oldEntry.Teachers = entry.Teachers;
                oldEntry.StartTime = entry.StartTime;
                oldEntry.Length = entry.Length;
                oldEntry.SubGroupNumber = entry.SubGroupNumber;

                entry.Id = oldEntry.Id;
                existingLessons.Add(oldEntry);
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