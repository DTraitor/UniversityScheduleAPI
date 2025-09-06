using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IScheduleLessonRepository
{
    void AddRange(IEnumerable<ScheduleLesson> toAdd);
    void RemoveByGroupId(int groupId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<IEnumerable<ScheduleLesson>> GetByGroupIdAsync(int groupId, CancellationToken stoppingToken);
}