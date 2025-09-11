using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces;

public interface IElectiveLessonRepository : IKeyBasedRepository<ElectiveLesson>
{
    void RemoveAll();
}