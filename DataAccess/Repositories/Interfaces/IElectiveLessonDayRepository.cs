using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonDayRepository : IRepository<ElectiveLessonDay>
{
    Task<IEnumerable<ElectiveLessonDay>> GetByIdsAsync(IEnumerable<int> ids);
}