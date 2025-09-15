using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectedLessonRepository : IRepository<ElectedLesson>
{
    Task<IEnumerable<ElectedLesson>> GetByElectiveDayIdAsync(int electiveDayId);
    Task<IEnumerable<ElectedLesson>> GetByIdsAsync(IEnumerable<int> ids);
}