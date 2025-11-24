using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository : IRepository<UserLesson>
{
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids);
    IEnumerable<int> RemoveByUserIds(IEnumerable<int> userIds);
    Task<IEnumerable<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(IEnumerable<int> userIds, SelectedLessonSourceType sourceType, IEnumerable<int> sourceId);
    Task<IList<UserLesson>> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}