using DataAccess.Models;

namespace BusinessLogic.Parsing.Interfaces;

public interface IScheduleReader
{
    Task<ICollection<LessonEntry>?> ReadSchedule(LessonSource source, CancellationToken cancellationToken);
}