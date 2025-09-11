using DataAccess.Enums;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository
{
    Task AddRangeAsync(IEnumerable<UserLesson> lessons);
    void ClearByUserId(int userId);
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IEnumerable<int> RemoveByUserIdAndLessonSourceType(int userId, LessonSourceTypeEnum sourceType);
    IEnumerable<UserLesson> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}