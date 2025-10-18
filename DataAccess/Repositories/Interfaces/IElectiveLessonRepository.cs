using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository : IKeyBasedRepository<ElectiveLesson>
{
    Task<IEnumerable<int>> GetUniqueLessonDaysAsync();
    Task<IEnumerable<ElectiveLesson>> GetByDayIdAndPartialNameAsync(int dayId, string partialName);
    Task<IEnumerable<ElectiveLesson>> GetByElectiveDayIdsAsync(IEnumerable<int> electiveDayIds);
    Task<IEnumerable<ElectiveLesson>> GetByIdsAsync(IEnumerable<int> ids);
}