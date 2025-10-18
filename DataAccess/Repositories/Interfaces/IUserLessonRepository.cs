using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository : IRepository<UserLesson>
{
    Task AddRangeAsync(IEnumerable<UserLesson> lessons);
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids);
    IEnumerable<int> RemoveByUserIdsAndLessonSourceType(IEnumerable<int> userIds, LessonSourceTypeEnum sourceType);
    Task<IEnumerable<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(IEnumerable<int> userIds, LessonSourceTypeEnum sourceType, IEnumerable<int> sourceId);
    IEnumerable<UserLesson> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}