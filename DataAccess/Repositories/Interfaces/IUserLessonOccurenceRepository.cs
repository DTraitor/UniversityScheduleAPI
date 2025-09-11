using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository
{
    void AddRange(IEnumerable<UserLessonOccurrence> lessonOccurrences);
    void ClearByLessonIds(IEnumerable<int> toRemove);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
    Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndDateAsync(int userId, DateTimeOffset date);
    UserLessonOccurrence? GetLatestOccurrence(int lessonId);
}