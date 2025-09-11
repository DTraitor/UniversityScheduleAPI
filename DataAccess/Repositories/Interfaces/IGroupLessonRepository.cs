using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupLessonRepository : IKeyBasedRepository<GroupLesson>
{
    Task<IEnumerable<GroupLesson>> GetByGroupIdAsync(int groupId, CancellationToken stoppingToken);
}