using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository : IRepository<UserLesson>
{
    Task AddRangeAsync(IEnumerable<UserLesson> lessons);
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids);
    IEnumerable<int> RemoveByUserIdAndLessonSourceType(int userId, LessonSourceTypeEnum sourceType);
    IEnumerable<int> RemoveByUserIdAndLessonSourceTypeAndLessonSourceId(int userId, LessonSourceTypeEnum sourceType, int sourceId);
    IEnumerable<UserLesson> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}