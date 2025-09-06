using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IScheduleLessonRepository
{
    Task AddRangeAsync(IEnumerable<ScheduleLesson> toAdd, CancellationToken stoppingToken);
    void RemoveAll();
    void RemoveByGroupId(int groupId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}