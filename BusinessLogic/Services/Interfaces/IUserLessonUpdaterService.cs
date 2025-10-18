using DataAccess.Enums;
using DataAccess.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUserLessonUpdaterService<T>
{
    public Task ProcessModifiedUser(IEnumerable<UserModified> modifiedUser);

    public ProcessedByEnum ProcessedBy { get; }
}