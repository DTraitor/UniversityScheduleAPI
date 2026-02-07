using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISelectedLessonSourceRepository : IRepository<SelectedLessonSource>
{
    Task<ICollection<SelectedLessonSource>> GetByUserId(int userId);
    Task<ICollection<SelectedLessonSource>> GetByUserIds(ICollection<int> userIds);
    Task<ICollection<SelectedLessonSource>> GetBySourceIds(ICollection<int> sourceIds);
    Task<ICollection<SelectedLessonSource>> GetByUserIdAndSourceType(int userId, LessonSourceType lessonSourceType);
    Task<ICollection<SelectedLessonSource>> GetByUserIdsAndSourceType(ICollection<int> userIds, LessonSourceType lessonSourceType);
}