using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonEntryRepository : IRepository<LessonEntry>
{
    Task<IEnumerable<LessonEntry>> GetBySourceIdsAsync(IEnumerable<int> sourceIds);
}