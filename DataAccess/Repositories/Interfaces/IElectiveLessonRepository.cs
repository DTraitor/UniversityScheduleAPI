using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository : IKeyBasedRepository<ElectiveLesson>
{
    Task<IEnumerable<ElectiveLesson>> GetByElectiveDayIdAsync(int electiveDayId);
    Task<IEnumerable<ElectiveLesson>> GetByIdsAsync(IEnumerable<int> ids);
}