using Common.Models;
using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IElectiveService
{
     Task<ICollection<LessonSource>> GetPossibleLevelsAsync();
     Task<Result<ICollection<string>>> GetLessonsByNameAsync(string lessonName, int sourceId);
     Task<Result<ICollection<string>>> GetLessonTypesAsync(string lessonName, int sourceId);
     Task<Result<ICollection<int>>> GetLessonSubgroupsAsync(int sourceId, string lessonName, string lessonType);
}