using Common.Models;

namespace BusinessLogic.Services.Interfaces;

public interface ILessonUpdaterService
{
    Task ProcessModifiedEntry(ICollection<LessonSourceModified> modifiedEntry);
}