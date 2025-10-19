using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IElectiveService
{
     Task<IEnumerable<ElectiveLessonDto>> GetLessons(string lessonName);
     Task<ElectiveSubgroupsDto> GetPossibleSubgroups(int lessonSourceId, string lessonType);
     Task<ElectiveLessonDayDto> GetPossibleDays(int lessonSourceId);
     Task AddSelectedSource(long telegramId, int lessonSourceId, string lessonType, int subgroupNumber);
     Task RemoveSelectedSource(long telegramId, int selectedSource);
     Task AddSelectedEntry(long telegramId, int lessonSourceId, int lessonEntry);
     Task RemoveSelectedEntry(long telegramId, int selectedEntry);
     Task<ElectiveSelectedLessonDto> GetUserLessons(long telegramId);
}