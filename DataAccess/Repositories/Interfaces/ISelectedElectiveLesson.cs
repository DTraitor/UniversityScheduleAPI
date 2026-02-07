using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISelectedElectiveLesson : IRepository<SelectedElectiveLesson>
{
    Task<IEnumerable<SelectedElectiveLesson>> GetBySourceIds(IEnumerable<int> sourceIds);
    Task<IEnumerable<SelectedElectiveLesson>> GetByUserIds(IEnumerable<int> userIds);
    Task<IEnumerable<SelectedElectiveLesson>> GetByUserId(int userId);
}