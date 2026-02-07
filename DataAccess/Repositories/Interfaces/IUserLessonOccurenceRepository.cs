using System.Data.Common;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository : IRepository<UserLessonOccurrence>
{
    void ClearByLessonIds(IEnumerable<int> toRemove, DbTransaction transaction);
    Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndBetweenDateAsync(int userId, DateTimeOffset beginDate, DateTimeOffset endDate);
    UserLessonOccurrence? GetLatestOccurrence(int lessonId);
}