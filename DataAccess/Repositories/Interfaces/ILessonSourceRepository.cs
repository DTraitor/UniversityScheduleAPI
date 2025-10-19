using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonSourceRepository : IRepository<LessonSource>
{
    Task<IEnumerable<LessonSource>> GetByIdsAsync(IEnumerable<int> ids);
}