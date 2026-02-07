using Common.Enums;
using Common.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonSourceRepository : IRepository<LessonSource>
{
    Task<LessonSource?> GetByNameAndSourceTypeAsync(string name, LessonSourceType lessonSourceType);
    Task<ICollection<LessonSource>> GetByNameAndLimitAsync(string name, int limit);
    Task<ICollection<LessonSource>> GetByIdsAsync(ICollection<int> ids);
}