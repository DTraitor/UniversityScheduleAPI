using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IGroupLessonRepository : IRepository<GroupLesson>
{
    Task<IEnumerable<GroupLesson>> GetByGroupIdsAsync(IEnumerable<int> groupIds, CancellationToken stoppingToken = default);
}