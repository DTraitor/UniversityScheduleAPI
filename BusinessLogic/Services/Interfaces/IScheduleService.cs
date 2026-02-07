using Common.Models.Internal;
using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IScheduleService
{
    Task<Result<ICollection<UserLesson>, (DateTimeOffset, DateTimeOffset)>> GetScheduleForDate(DateTimeOffset dateTime, long userTelegramId);
}