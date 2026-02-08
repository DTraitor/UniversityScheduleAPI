using BusinessLogic.DTO;
using Common.Models;
using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IUserService
{
    Task CreateUserAsync(long telegramId);
    Task<Result> ChangeGroupAsync(long telegramId, string groupName);
    Task<Result<ICollection<SelectedElectiveLesson>>> GetUserElectiveLessonAsync(long telegramId);
    Task<Result> AddUserElectiveLessonAsync(long telegramId, int sourceId, string lessonName, string? lessonType, int subgroupNumber);
    Task RemoveUserElectiveLessonAsync(long telegramId, int electiveLessonId);

    Task<bool> UserExists(long telegramId);
}