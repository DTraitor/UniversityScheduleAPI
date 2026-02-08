using Common.Models;

namespace BusinessLogic.Parsing.Interfaces;

public interface IChangeHandler
{
    Task<ICollection<LessonEntry>> HandleChanges(IEnumerable<LessonEntry> oldLessons, ICollection<LessonEntry> newLessons, CancellationToken token);
}