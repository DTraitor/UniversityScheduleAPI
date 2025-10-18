using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupLessonRepository : IKeyBasedRepository<GroupLesson>
{
    Task<IEnumerable<GroupLesson>> GetByGroupIdsAsync(IEnumerable<int> groupIds, CancellationToken stoppingToken = default);
}