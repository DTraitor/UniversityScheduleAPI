using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository
{
    void AddRange(IEnumerable<UserLessonOccurrence> lessonOccurrences);
    void ClearByUserId(int userId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAsync(int userId);
    UserLessonOccurrence? GetLatestOccurrence(int lessonId);
}