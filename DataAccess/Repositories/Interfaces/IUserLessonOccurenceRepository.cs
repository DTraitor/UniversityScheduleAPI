using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonOccurenceRepository
{
    Task AddRangeAsync(IEnumerable<UserLessonOccurrence> lessonOccurences);
    void ClearByUserId(int userId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}