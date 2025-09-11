using DataAccess.Models.Interface;

namespace BusinessLogic.Services.Interfaces;

public interface ILessonUpdaterService<T, TModifiedEntry> where TModifiedEntry : IModifiedEntry
{
    Task ProcessModifiedEntry(TModifiedEntry modifiedEntry);
}