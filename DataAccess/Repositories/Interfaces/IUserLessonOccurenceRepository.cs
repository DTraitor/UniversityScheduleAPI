using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository : IRepository<UserLessonOccurrence>
{
    void ClearByLessonIds(IEnumerable<int> toRemove);
    Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndDateAsync(int userId, DateTimeOffset date);
    UserLessonOccurrence? GetLatestOccurrence(int lessonId);
}