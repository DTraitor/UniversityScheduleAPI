using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupLessonRepository : IRepository<GroupLesson>
{
    void RemoveByGroupId(int groupId);
    Task<IEnumerable<GroupLesson>> GetByGroupIdAsync(int groupId, CancellationToken stoppingToken);
}