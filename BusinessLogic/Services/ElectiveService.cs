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

    public async Task AddSelectedSource(long telegramId, int lessonSourceId, string lessonType, int subgroupNumber)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var lessonSource = await _lessonSourceRepository.GetByIdAsync(lessonSourceId);
        if (lessonSource == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonSource.SourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");

        _selectedLessonSourceRepository.Add(new SelectedLessonSource
        {
            LessonSourceType = LessonSourceType.Elective,
            SourceId = lessonSourceId,
            SourceName = lessonSource.Name,
            Type = lessonType,
            SubGroupNumber = subgroupNumber,
            UserId = user.Id,
        });

        _userModifiedRepository.Add(user.Id);

        await _selectedLessonSourceRepository.SaveChangesAsync();
        await _userModifiedRepository.SaveChangesAsync();
    }

    public async Task RemoveSelectedSource(long telegramId, int selectedSource)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return;

        var lessonSource = await _selectedLessonSourceRepository.GetByIdAsync(selectedSource);
        if (lessonSource == null)
            return;
        if (lessonSource.LessonSourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");
        if (lessonSource.UserId != user.Id)
            throw new InvalidOperationException("User is not elective owner");

        _selectedLessonSourceRepository.Delete(lessonSource);
        _userModifiedRepository.Add(user.Id);

        await _selectedLessonSourceRepository.SaveChangesAsync();
        await _userModifiedRepository.SaveChangesAsync();
    }

    public async Task AddSelectedEntry(long telegramId, int lessonSourceId, int lessonEntry)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var lessonSource = await _lessonSourceRepository.GetByIdAsync(lessonSourceId);
        if (lessonSource == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonSource.SourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");

        var entry = await _lessonEntryRepository.GetByIdAsync(lessonEntry);
        if (entry == null)
            throw new KeyNotFoundException("Selected entry not found");
        if (entry.SourceId != lessonSource.Id)
            throw new InvalidOperationException("Selected entry source not found");

        _selectedLessonEntryRepository.Add(new SelectedLessonEntry
        {
            EntryId = entry.Id,
            SourceId = lessonSourceId,
            EntryName = lessonSource.Name,
            Type = entry.Type,
            WeekNumber = entry.Week,
            DayOfWeek = entry.DayOfWeek,
            StartTime = entry.StartTime,
            UserId = user.Id,
        });

        _userModifiedRepository.Add(user.Id);

        await _selectedLessonEntryRepository.SaveChangesAsync();
        await _userModifiedRepository.SaveChangesAsync();
    }

    public async Task RemoveSelectedEntry(long telegramId, int selectedEntry)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return;

        var lessonEntry = await _selectedLessonEntryRepository.GetByIdAsync(selectedEntry);
        if (lessonEntry == null)
            return;
        if (lessonEntry.UserId != user.Id)
            throw new InvalidOperationException("User is not elective owner");

        _selectedLessonEntryRepository.Delete(lessonEntry);
        _userModifiedRepository.Add(user.Id);

        await _selectedLessonEntryRepository.SaveChangesAsync();
        await _userModifiedRepository.SaveChangesAsync();
    }

    public async Task<ElectiveSelectedLessonDto> GetUserLessons(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var selectedSources = (await _selectedLessonSourceRepository.GetByUserId(user.Id)).Where(x => x.LessonSourceType != LessonSourceType.Group);
        var selectedEntries = await _selectedLessonEntryRepository.GetByUserId(user.Id);

        return new ElectiveSelectedLessonDto
        {
            Sources = selectedSources.Select(x => new ElectiveSelectedLessonDto.SourceDto
            {
                Name = x.SourceName,
                SelectedSourceId = x.Id,
                Type = x.Type,
                SubGroupNumber = x.SubGroupNumber,
            }),
            Entries = selectedEntries.Select(x => new ElectiveSelectedLessonDto.EntryDto
            {
                DayOfWeek = x.DayOfWeek,
                EntryName = x.EntryName,
                SelectedEntryId = x.Id,
                Type = x.Type,
                StartTime = x.StartTime,
                WeekNumber = x.WeekNumber,
            })
        };
    }
}