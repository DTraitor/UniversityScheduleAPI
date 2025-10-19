using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISelectedLessonEntryRepository : IRepository<SelectedLessonEntry>
{
    Task<IEnumerable<SelectedLessonEntry>> GetBySourceIds(IEnumerable<int> sourceIds);
    Task<IEnumerable<SelectedLessonEntry>> GetByUserIds(IEnumerable<int> userIds);
}