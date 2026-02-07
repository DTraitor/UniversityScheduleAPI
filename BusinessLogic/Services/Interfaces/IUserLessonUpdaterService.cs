using Common.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUserLessonUpdaterService
{
    public Task ProcessModifiedUser(IEnumerable<UserModified> modifiedUser);
}