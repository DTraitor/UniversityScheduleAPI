using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IElectiveService
{
     Task<IEnumerable<ElectiveLessonDayDto>> GetPossibleDays();
     Task<IEnumerable<ElectiveLessonDto>> GetElectiveLessons(int electiveDayId, string partialLessonName);
     Task<IEnumerable<ElectiveLessonDto>> GetCurrentLessons(long telegramId);
     Task CreateNewElectedLesson(CreateElectiveLessonDto newLesson);
     Task RemoveElectedLesson(long telegramId, int lessonId);
}