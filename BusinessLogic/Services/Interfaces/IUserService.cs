using Common.Models;
using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IUserService
{
    Task CreateUserAsync(long telegramId);
    Task<Result> ChangeGroupAsync(long telegramId, string groupName, int subgroupNumber);
    Task<Result<ICollection<SelectedElectiveLesson>>> GetUserElectiveLessonAsync(long telegramId);
    Task<Result> AddUserElectiveLessonAsync(long telegramId, int sourceId, string lessonName, string? lessonType, int subgroupNumber);
    Task<Result> RemoveUserElectiveLessonAsync(long telegramId, int electiveLessonId);
}