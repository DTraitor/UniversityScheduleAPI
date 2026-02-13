using Common.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ILessonEntryRepository : IRepository<LessonEntry>
{
    Task<ICollection<LessonEntry>> GetBySourceIdAsync(int sourceId);
    Task<ICollection<LessonEntry>> GetBySourceIdAndPartialNameAsync(int sourceId, string partialName);
    Task<ICollection<LessonEntry>> GetBySourceIdsAsync(ICollection<int> sourceIds);
}