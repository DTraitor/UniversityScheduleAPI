using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository
{
    void AddRange(IEnumerable<UserLessonOccurrence> lessonOccurrences);
    void ClearByUserId(int userId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndDateAsync(int userId, DateTimeOffset date);
    UserLessonOccurrence? GetLatestOccurrence(int lessonId);
}