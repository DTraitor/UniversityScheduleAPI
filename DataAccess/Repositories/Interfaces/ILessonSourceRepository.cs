using DataAccess.Enums;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonSourceRepository : IRepository<LessonSource>
{
    Task<LessonSource?> GetByNameAndSourceTypeAsync(string name, LessonSourceType lessonSourceType);
    Task<IEnumerable<LessonSource>> GetByIdsAsync(IEnumerable<int> ids);
}