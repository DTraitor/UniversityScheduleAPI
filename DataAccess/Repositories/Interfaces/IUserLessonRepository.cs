using Common.Enums;
using Common.Models.Internal;

namespace DataAccess.Repositories.Interfaces;

public interface IUserLessonRepository : IRepository<UserLesson>
{
    Task<ICollection<UserLesson>> GetByIdsAsync(ICollection<int> ids);
    Task<ICollection<int>> RemoveByUserIdsAsync(ICollection<int> userIds);
    Task<ICollection<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIdsAsync(ICollection<int> userIds, SelectedLessonSourceType sourceType, ICollection<int> sourceId);
    Task<ICollection<UserLesson>> GetWithOccurrencesCalculatedDateLessThanAsync(DateTimeOffset dateTime);
}