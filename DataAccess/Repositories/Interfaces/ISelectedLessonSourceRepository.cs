using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISelectedLessonSourceRepository : IRepository<SelectedLessonSource>
{
    Task<IEnumerable<SelectedLessonSource>> GetByUserIds(IEnumerable<int> userIds);
    Task<IEnumerable<SelectedLessonSource>> GetBySourceIds(IEnumerable<int> sourceIds);
    Task<IEnumerable<SelectedLessonSource>> GetByUserIdsAndSourceType(IEnumerable<int> userIds, LessonSourceType lessonSourceType);
}