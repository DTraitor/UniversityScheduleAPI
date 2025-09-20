using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IScheduleService
{
    Task<IEnumerable<LessonDto>> GetScheduleForDate(DateTimeOffset dateTime, long userTelegramId);
}