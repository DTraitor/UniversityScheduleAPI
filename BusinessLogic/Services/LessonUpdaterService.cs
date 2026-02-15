using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services;

public class LessonUpdaterService : ILessonUpdaterService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository  _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedElectiveLessonRepository selectedElectiveLessonsRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly IUserAlertService _userAlertService;
    private readonly ILogger<LessonUpdaterService> _logger;

    public LessonUpdaterService(
        ILessonSourceRepository lessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedElectiveLessonRepository selectedElectiveLessonsRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        IUserAlertService userAlertService,
        ILogger<LessonUpdaterService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        this.selectedElectiveLessonsRepository = selectedElectiveLessonsRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _userAlertService = userAlertService;
        _logger = logger;
    }

    private async Task RemoveOldLessons(ICollection<int> userIds, ICollection<int> selectedSources, ICollection<int> selectedElecties)
    {
        var removedBySource = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIdsAsync(
            userIds,
            SelectedLessonSourceType.Group,
            selectedSources);

        var removedElectives = await _userLessonRepository.RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIdsAsync(
            userIds,
            SelectedLessonSourceType.Elective,
            selectedElecties);

        await _userLessonOccurenceRepository.ClearByLessonIdsAsync(removedBySource.Union(removedElectives).ToList());
    }

    private async Task AlertUsersOfChanges(ICollection<SelectedLessonSource> selectedSources, ICollection<SelectedElectiveLesson> selectedElectiveLessons, HashSet<int> existingSourceIds)
    {
        foreach (var removedSource in selectedSources.Where(x => !existingSourceIds.Contains(x.SourceId)))
        {
            _userAlertService.CreateUserAlert(removedSource.UserId, UserAlertType.GroupRemoved, new()
            {
                { "LessonName", removedSource.SourceName },
                { "SubGroupNumber", removedSource.SubGroupNumber.ToString() },
            });
        }

        // Might've added alert for electives as well, but I don't want to spend time on it

        await _selectedLessonSourceRepository.RemoveRangeAsync(selectedSources.Where(x => !existingSourceIds.Contains(x.SourceId)).ToArray());
        await selectedElectiveLessonsRepository.RemoveRangeAsync(selectedElectiveLessons.Where(x => !existingSourceIds.Contains(x.LessonSourceId)).ToArray());
    }

    private ICollection<UserLesson> GetUserGroupLessons(User user, ICollection<SelectedLessonSource> selectedLessonSources,
        ICollection<LessonSource> sources, ICollection<LessonEntry> entries)
    {
        List<UserLesson> userLessons = new List<UserLesson>();

        foreach (var selectedSource in selectedLessonSources)
        {
            var source = sources.FirstOrDefault(x => x.Id == selectedSource.SourceId);

            var lessonsToMap = entries
                .Where(e => e.SourceId == source.Id);

            if (selectedSource.SubGroupNumber != -1)
                lessonsToMap = lessonsToMap
                    .Where(e => e.SubGroupNumber == selectedSource.SubGroupNumber || e.SubGroupNumber == -1);

            userLessons.AddRange(ScheduleLessonsMapper.Map(
                    lessonsToMap.ToList(),
                    source.StartDate,
                    source.EndDate,
                    TimeZoneInfo.FindSystemTimeZoneById(source.TimeZone))
                .Select(x =>
                {
                    x.UserId = user.Id;
                    x.SelectedLessonSourceType |= SelectedLessonSourceType.Group;
                    x.LessonSourceId = selectedSource.Id;
                    return x;
                }));
        }

        return userLessons;
    }

    private ICollection<UserLesson> GetUserElectiveLessons(User user, ICollection<SelectedElectiveLesson> selectedLessonSources,
        ICollection<LessonSource> sources, ICollection<LessonEntry> entries)
    {
        List<UserLesson> userLessons = new List<UserLesson>();

        foreach (var selectedElectiveLesson in selectedLessonSources)
        {
            var source = sources.FirstOrDefault(x => x.Id == selectedElectiveLesson.LessonSourceId);

            var entriesToMap = entries
                .Where(x => x.Title == selectedElectiveLesson.LessonName)
                .Where(x => x.SubGroupNumber == selectedElectiveLesson.SubgroupNumber);

            if (selectedElectiveLesson.LessonType is not null)
                entriesToMap.Where(x => x.Type == selectedElectiveLesson.LessonType);

            userLessons.AddRange(
                ScheduleLessonsMapper.Map(
                    entriesToMap.ToArray(),
                    source.StartDate,
                    source.EndDate,
                    TimeZoneInfo.FindSystemTimeZoneById(source.TimeZone)
                ).Select(x =>
                {
                    x.UserId = user.Id;
                    x.SelectedLessonSourceType |= SelectedLessonSourceType.Elective;
                    x.LessonSourceId = selectedElectiveLesson.Id;
                    return x;
                })
            );
        }

        return userLessons;
    }

    public async Task ProcessModifiedEntry(ICollection<LessonSourceModified> modifiedEntry)
    {
        HashSet<int> modifiedSourceIds = modifiedEntry.Select(x => x.SourceId).ToHashSet();
        var selectedSources = await _selectedLessonSourceRepository.GetBySourceIds(modifiedSourceIds);
        var selectedElectiveLessons = await selectedElectiveLessonsRepository.GetBySourceIds(modifiedSourceIds);

        var users = await _userRepository.GetByIdsAsync(
            selectedSources
                .Select(x => x.UserId)
                .Union(selectedElectiveLessons.Select(x => x.UserId))
                .ToArray()
            );

        if(users.Count == 0)
            return;

        var userIds = users.Select(x => x.Id).ToList();

        await using var transaction = await _userLessonRepository.BeginTransactionAsync();

        await RemoveOldLessons(
            userIds,
            selectedSources.Select(x => x.SourceId).ToArray(),
            selectedElectiveLessons.Select(x => x.LessonSourceId).ToArray());

        var sources = await _lessonSourceRepository.GetByIdsAsync(modifiedSourceIds);
        var entries = await _lessonEntryRepository.GetBySourceIdsAsync(modifiedSourceIds);

        var existingSourceIds = sources.Select(x => x.Id).ToHashSet();
        var existingEntriesIds = entries.Select(x => x.Id).ToHashSet();

        await AlertUsersOfChanges(selectedSources, selectedElectiveLessons, existingSourceIds);

        Dictionary<int, List<SelectedLessonSource>> userIdToSelectedSources = selectedSources
            .Where(x => existingSourceIds.Contains(x.SourceId))
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());
        Dictionary<int, List<SelectedElectiveLesson>> userIdToSelectedEntries = selectedElectiveLessons
            .Where(x => existingSourceIds.Contains(x.LessonSourceId))
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());

        List<UserLesson> userLessons = new List<UserLesson>();

        foreach (var user in users)
        {
            if (userIdToSelectedSources.TryGetValue(user.Id, out var selectedSourcesDict))
            {
                userLessons.AddRange(GetUserGroupLessons(user, selectedSourcesDict, sources, entries));
            }

            if (userIdToSelectedEntries.TryGetValue(user.Id, out var selectedEntriesDict))
            {
                userLessons.AddRange(GetUserElectiveLessons(user, selectedEntriesDict, sources, entries));
            }
        }

        await _userLessonRepository.AddRangeAsync(userLessons);

        await _selectedLessonSourceRepository.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}