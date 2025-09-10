using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository : IRepository<ElectiveLesson>
{
    void RemoveAll();
}