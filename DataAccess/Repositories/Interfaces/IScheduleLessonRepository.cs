using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IScheduleLessonRepository
{
    Task AddRangeAsync(IEnumerable<ScheduleLesson> toAdd, CancellationToken stoppingToken);
    void RemoveAll();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}