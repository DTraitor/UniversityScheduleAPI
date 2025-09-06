using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository
{
    Task<IEnumerable<ElectiveLesson>> GetByUserIdAsync(int userId, CancellationToken stoppingToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}