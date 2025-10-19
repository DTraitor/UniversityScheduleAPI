using BusinessLogic.Configuration;
using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services;

public class LessonUpdaterService : ILessonUpdaterService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository  _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedLessonEntryRepository _selectedLessonEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IOptions<ElectiveScheduleParsingOptions> _options;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<LessonUpdaterService> _logger;

    public LessonUpdaterService(
        ILessonSourceRepository lessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedLessonEntryRepository selectedLessonEntryRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IOptions<ElectiveScheduleParsingOptions> options,
        IUserAlertService userAlertService,
        ILogger<LessonUpdaterService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _selectedLessonEntryRepository = selectedLessonEntryRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _options = options;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    public async Task ProcessModifiedEntry(IEnumerable<LessonSourceModified> modifiedEntry)
    {
        HashSet<int> modifiedSourceIds = modifiedEntry.Select(x => x.SourceId).ToHashSet();
        var selectedSources = await _selectedLessonSourceRepository.GetBySourceIds(modifiedSourceIds);
        var selectedEntries = await _selectedLessonEntryRepository.GetBySourceIds(modifiedSourceIds);

        var sourceIds = selectedSources
            .Select(x => x.SourceId)
            .Union(selectedEntries.Select(x => x.SourceId))
            .ToHashSet();

        var users = await _userRepository.GetByIdsAsync(
            selectedSources
                .Select(x => x.UserId)
                .Union(selectedEntries.Select(x => x.UserId))
            );

        if(!users.Any())
            return;

        var removedBySource = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(
            users.Select(x => x.Id),
            SelectedLessonSourceType.Source,
            selectedSources.Select(x => x.Id));

        var removedByEntry = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(
            users.Select(x => x.Id),
            SelectedLessonSourceType.Source,
            selectedEntries.Select(x => x.Id));

        _userLessonOccurenceRepository.ClearByLessonIds(removedBySource.Union(removedByEntry));

        var sources = await _lessonSourceRepository.GetByIdsAsync(sourceIds);
        var entries = await _lessonEntryRepository.GetBySourceIdsAsync(sourceIds);

        var existingSourceIds = sources.Select(x => x.Id).ToHashSet();
        var existingEntriesIds = entries.Select(x => x.Id).ToHashSet();

        foreach (var removedSource in selectedSources.Where(x => !existingSourceIds.Contains(x.Id)))
        {
            _userAlertService.CreateUserAlert(removedSource.UserId, UserAlertType.SourceRemoved, new()
            {
                { "LessonName", removedSource.SourceName },
            });
        }

        foreach (var removedEntry in selectedEntries.Where(x => !existingEntriesIds.Contains(x.Id)))
        {
            _userAlertService.CreateUserAlert(removedEntry.UserId, UserAlertType.EntryRemoved, new()
            {
                { "LessonName", removedEntry.EntryName },
                { "LessonType", removedEntry.Type ?? "" },
                { "LessonStartTime", removedEntry.StartTime.ToString() },
                { "LessonWeek", removedEntry.WeekNumber.ToString() },
                { "LessonDay", removedEntry.DayOfWeek.ToString() },
            });
        }

        Dictionary<int, List<SelectedLessonSource>> userIdToSelectedSources = selectedSources
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());
        Dictionary<int, List<SelectedLessonEntry>> userIdToSelectedEntries = selectedEntries
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());

        List<UserLesson> userLessons = new List<UserLesson>();

        foreach (var user in users)
        {
            foreach (var selectedSource in userIdToSelectedSources[user.Id])
            {
                var source = sources.FirstOrDefault(x => x.Id == selectedSource.SourceId);

                var lessonsToMap = entries
                    .Where(e => e.SourceId == source.Id);

                if (selectedSource.SubGroupNumber != -1)
                    lessonsToMap = lessonsToMap
                        .Where(e => e.SubGroupNumber == selectedSource.SubGroupNumber || e.SubGroupNumber == -1);

                userLessons.AddRange(
                    ScheduleLessonsMapper.Map(
                        lessonsToMap.ToList(),
                        source.StartDate,
                        source.EndDate,
                        TimeZoneInfo.FindSystemTimeZoneById(source.TimeZone)
                    ).Select(x =>
                    {
                        x.SelectedLessonSourceType = SelectedLessonSourceType.Source;
                        x.LessonSourceId = selectedSource.Id;
                        return x;
                    })
                );
            }

            foreach (var selectedEntry in userIdToSelectedEntries[user.Id].GroupBy(x => x.SourceId))
            {
                var entriesIds = selectedEntry.Select(x => x.EntryId).ToHashSet();
                var source = sources.FirstOrDefault(x => x.Id == selectedEntry.Key);

                userLessons.AddRange(
                    ScheduleLessonsMapper.Map(
                        entries
                            .Where(e => entriesIds.Contains(e.Id))
                            .ToList(),
                        source.StartDate,
                        source.EndDate,
                        TimeZoneInfo.FindSystemTimeZoneById(source.TimeZone)
                    ).Select(x =>
                    {
                        x.SelectedLessonSourceType = SelectedLessonSourceType.Entry;
                        x.LessonSourceId = selectedEntry.Key;
                        return x;
                    })
                );
            }
        }

        _userLessonRepository.AddRange(userLessons);

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }
}