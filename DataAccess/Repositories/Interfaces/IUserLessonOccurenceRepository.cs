using System.Data.Common;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository : IRepository<UserLessonOccurrence>
{
    Task ClearByLessonIdsAsync(ICollection<int> toRemove);
    Task<ICollection<UserLessonOccurrence>> GetByUserIdAndBetweenDateAsync(int userId, DateTimeOffset beginDate, DateTimeOffset endDate);
    Task<UserLessonOccurrence?> GetLatestOccurrenceAsync(int lessonId);
}