using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
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
    private readonly ISelectedLessonEntryRepository _selectedLessonEntryRepository;
    private readonly ILogger<ElectiveService> _logger;

    public ElectiveService(
        ILessonSourceRepository lessonSourceRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        IUserModifiedRepository userModifiedRepository,
        ISelectedLessonEntryRepository selectedLessonEntryRepository,
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

    public async Task<IEnumerable<ElectiveLessonDto>> GetLessons(string lessonName)
    {
        var lessonSources = await _lessonSourceRepository.GetAllLimitAsync(11);
        if (lessonSources.Count() >= 11)
            throw new InvalidOperationException("Should be more specific");

        var entries = (await _lessonEntryRepository.GetBySourceIdsAsync(lessonSources.Select(x => x.Id)))
            .GroupBy(x => x.SourceId);

        return lessonSources.Select(x => new ElectiveLessonDto
        {
            Title = x.Name,
            SourceId = x.Id,
            Types = entries
                .Where(y => y.Key == x.Id)
                .Select(y => y.FirstOrDefault()?.Type ?? "")
                .Distinct()
                .ToList()

        });
    }

    public async Task<ElectiveSubgroupsDto> GetPossibleSubgroups(int lessonSourceId, string lessonType)
    {
        var lessonSource = await _lessonSourceRepository.GetByIdAsync(lessonSourceId);
        if (lessonSource == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonSource.SourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");

        var entries = (await _lessonEntryRepository.GetBySourceIdAsync(lessonSourceId)).GroupBy(x => x.SubGroupNumber);

        return new ElectiveSubgroupsDto()
        {
            LessonSourceId = lessonSourceId,
            PossibleSubgroups = entries.Select(z => z.Key).Distinct().Append(-1)
        };
    }

    public async Task<ElectiveLessonDayDto> GetPossibleDays(int lessonSourceId)
    {
        var lessonSource = await _lessonSourceRepository.GetByIdAsync(lessonSourceId);
        if (lessonSource == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonSource.SourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");

        var entries = await _lessonEntryRepository.GetBySourceIdAsync(lessonSourceId);

        return new ElectiveLessonDayDto
        {
            SourceId = lessonSourceId,
            LessonDays = entries.Select(x => new ElectiveLessonDayDto.ElectiveLessonSpecificDto
            {
                Type = x.Type,
                DayOfWeek = x.DayOfWeek,
                StartTime = x.StartTime,
                WeekNumber = x.Week
            })
        };
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
            throw new KeyNotFoundException("User not found");

        var lessonSource = await _selectedLessonSourceRepository.GetByIdAsync(selectedSource);
        if (lessonSource == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonSource.LessonSourceType != LessonSourceType.Elective)
            throw new InvalidOperationException("Lesson source type is not elective");
        if (lessonSource.UserId != user.Id)
            throw new InvalidOperationException("User is not elective");

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
        if (entry.SourceId == lessonSourceId)
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
            throw new KeyNotFoundException("User not found");

        var lessonEntry = await _selectedLessonEntryRepository.GetByIdAsync(selectedEntry);
        if (lessonEntry == null)
            throw new KeyNotFoundException("Lesson not found");
        if (lessonEntry.UserId != user.Id)
            throw new InvalidOperationException("User is not elective");

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

        var selectedSources = await _selectedLessonSourceRepository.GetByUserId(user.Id);
        var selectedEntries = await _selectedLessonEntryRepository.GetByUserId(user.Id);

        return new ElectiveSelectedLessonDto
        {
            Sources = selectedSources.Select(x => new ElectiveSelectedLessonDto.SourceDto
            {
                Name = x.SourceName,
                SelectedSourceId = x.Id,
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