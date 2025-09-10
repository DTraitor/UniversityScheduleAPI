using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    void AddRange(IEnumerable<ElectiveLesson> lessons);
}