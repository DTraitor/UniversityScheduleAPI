using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Result;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class ElectiveService : IElectiveService
{
    private readonly IUserModifiedRepository _userModifiedRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedElectiveLesson _selectedLessonEntryRepository;
    private readonly ILogger<ElectiveService> _logger;

    public ElectiveService(
        ILessonSourceRepository lessonSourceRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        IUserModifiedRepository userModifiedRepository,
        ISelectedElectiveLesson selectedLessonEntryRepository,
        IUserRepository userRepository,
        ILogger<ElectiveService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _userModifiedRepository = userModifiedRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonEntryRepository = selectedLessonEntryRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ICollection<LessonSource>> GetPossibleLevelsAsync()
    {
        return await _lessonSourceRepository.GetBySourceTypeAsync(LessonSourceType.Elective);
    }

    public async Task<Result<ICollection<string>>> GetLessonsByNameAsync(string lessonName, int sourceId)
    {
        var source = await _lessonSourceRepository.GetByIdAsync(sourceId);
        if (source == null || source.SourceType != LessonSourceType.Elective)
            return ErrorType.NotFound;

        var matchingLessons = (await _lessonEntryRepository.GetBySourceIdAndPartialNameAsync(sourceId, lessonName)).Select(x => x.Title).Distinct().ToArray();

        if (matchingLessons.Length > 5)
            return ErrorType.TooManyElements;

        return matchingLessons;
    }

    public async Task<Result<ICollection<string?>>> GetLessonTypesAsync(string lessonName, int sourceId)
    {
        var source = await _lessonSourceRepository.GetByIdAsync(sourceId);
        if (source == null || source.SourceType != LessonSourceType.Elective)
            return ErrorType.NotFound;

        var matchingLessons = await _lessonEntryRepository.GetBySourceIdAndPartialNameAsync(sourceId, lessonName);

        if (matchingLessons.Count <= 0 || matchingLessons.Any(x => x.Title != lessonName))
            return ErrorType.NotFound;

        return matchingLessons.Select(x => x.Type).Distinct().ToArray();
    }

    public async Task<Result<ICollection<int>>> GetLessonSubgroupsAsync(int sourceId, string lessonName, string? lessonType)
    {
        var source = await _lessonSourceRepository.GetByIdAsync(sourceId);
        if (source == null || source.SourceType != LessonSourceType.Elective)
            return ErrorType.NotFound;

        var matchingLessons = (await _lessonEntryRepository.GetBySourceIdAndPartialNameAsync(sourceId, lessonName)).Where(x => x.Type == lessonType).ToArray();

        if (matchingLessons.Length <= 0 || matchingLessons.Any(x => x.Title != lessonName))
            return ErrorType.NotFound;

        return matchingLessons.Select(x => x.SubGroupNumber).Distinct().ToArray();
    }
}
