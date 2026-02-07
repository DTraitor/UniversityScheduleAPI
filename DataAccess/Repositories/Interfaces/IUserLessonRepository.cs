using System.Data.Common;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository : IRepository<UserLesson>
{
    Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids, DbTransaction transaction);
    IEnumerable<int> RemoveByUserIds(IEnumerable<int> userIds, DbTransaction transaction);
    Task<IEnumerable<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(IEnumerable<int> userIds, SelectedLessonSourceType sourceType, IEnumerable<int> sourceId, DbTransaction transaction);
    Task<IList<UserLesson>> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime);
}