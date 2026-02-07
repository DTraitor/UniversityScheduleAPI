using Common.Models;

namespace DataAccess.Repositories.Interfaces;

public interface ISelectedElectiveLesson : IRepository<SelectedElectiveLesson>
{
    Task<ICollection<SelectedElectiveLesson>> GetBySourceIds(ICollection<int> sourceIds);
    Task<ICollection<SelectedElectiveLesson>> GetByUserIds(ICollection<int> userIds);
    Task<ICollection<SelectedElectiveLesson>> GetByUserId(int userId);
}