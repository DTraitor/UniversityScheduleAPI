using DataAccess.Models.Interface;

namespace BusinessLogic.Services.Interfaces;

public interface ILessonUpdaterService<T, in TModifiedEntry> where TModifiedEntry : IModifiedEntry
{
    Task ProcessModifiedEntry(IEnumerable<TModifiedEntry> modifiedEntry);
}