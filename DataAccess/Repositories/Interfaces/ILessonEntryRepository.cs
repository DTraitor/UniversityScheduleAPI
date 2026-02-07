using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonEntryRepository : IRepository<LessonEntry>
{
    Task<ICollection<LessonEntry>> GetBySourceIdAsync(int sourceId);
    Task<ICollection<LessonEntry>> GetBySourceIdsAsync(ICollection<int> sourceIds);
}