using DataAccess.Models;

namespace BusinessLogic.Parsing.Interfaces;

public interface IChangeHandler
{
    Task<IEnumerable<LessonEntry>> HandleChanges(IEnumerable<LessonEntry> oldLessons, ICollection<LessonEntry> newLessons, CancellationToken token);
}