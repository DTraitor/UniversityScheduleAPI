using BusinessLogic.DTO;
using Common.Models;
using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IElectiveService
{
     Task<ICollection<LessonSource>> GetPossibleLevelsAsync();
     Task<Result<ICollection<string>>> GetLessonsByNameAsync(string lessonName, int sourceId);
     Task<Result<ICollection<string>>> GetLessonTypesAsync(string lessonName, int sourceId);
     Task<Result<ICollection<int>>> GetLessonSubgroupsAsync(int sourceId, string lessonName, string lessonType);

     Task<ElectiveSubgroupsDto> GetPossibleSubgroups(int lessonSourceId, string lessonType);
     Task<ElectiveLessonDayDto> GetPossibleDays(int lessonSourceId);
     Task AddSelectedSource(long telegramId, int lessonSourceId, string lessonType, int subgroupNumber);
     Task RemoveSelectedSource(long telegramId, int selectedSource);
     Task AddSelectedEntry(long telegramId, int lessonSourceId, int lessonEntry);
     Task RemoveSelectedEntry(long telegramId, int selectedEntry);
     Task<ElectiveSelectedLessonDto> GetUserLessons(long telegramId);
}