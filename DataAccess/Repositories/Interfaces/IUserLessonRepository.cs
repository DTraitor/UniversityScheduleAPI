using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository
{
    Task AddRangeAsync(IEnumerable<UserLesson> lessons);
    void ClearByUserId(int userId);
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids);
    IEnumerable<UserLesson> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}