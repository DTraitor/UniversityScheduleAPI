using Common.Models;

namespace BusinessLogic.Services.Interfaces;

public interface ILessonUpdaterService
{
    Task ProcessModifiedEntry(IEnumerable<LessonSourceModified> modifiedEntry);
}