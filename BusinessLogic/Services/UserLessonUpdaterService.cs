using BusinessLogic.Mappers;
using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserLessonUpdaterService : IUserLessonUpdaterService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository  _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedElectiveLessonRepository _selectedElectiveLessonRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<UserLessonUpdaterService> _logger;

    public UserLessonUpdaterService(
        ILessonSourceRepository lessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedElectiveLessonRepository selectedElectiveLessonRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<UserLessonUpdaterService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _selectedElectiveLessonRepository = selectedElectiveLessonRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    private async Task RemoveOldLessons(ICollection<int> userIds)
    {
        var removed = await _userLessonRepository.RemoveByUserIdsAsync(userIds);
        await _userLessonOccurenceRepository.ClearByLessonIdsAsync(removed);
    }

    private ICollection<UserLesson> GetUserGroupLessons(User user,
        ICollection<SelectedLessonSource> selectedLessonSources,
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

    private ICollection<UserLesson> GetUserElectiveLessons(User user,
        ICollection<SelectedElectiveLesson> selectedLessonSources,
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
                entriesToMap = entriesToMap.Where(x => x.Type == selectedElectiveLesson.LessonType);

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

    public async Task ProcessModifiedUser(ICollection<UserModified> modifiedUsers)
    {
        var users = await _userRepository.GetByIdsAsync(modifiedUsers.Select(u => u.UserId).ToArray());
        var usersIds = users.Select(u => u.Id).ToHashSet();

        await using var transaction = await _userRepository.BeginTransactionAsync();

        await RemoveOldLessons(usersIds);

        var selectedSources = await _selectedLessonSourceRepository.GetByUserIds(usersIds);
        var selectedElectives = await _selectedElectiveLessonRepository.GetByUserIds(usersIds);

        Dictionary<int, List<SelectedLessonSource>> userIdToSelectedSources = selectedSources
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());
        Dictionary<int, List<SelectedElectiveLesson>> userIdToSelectedEntries = selectedElectives
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());

        var sources = await _lessonSourceRepository.GetByIdsAsync(
            selectedSources
                .Select(s => s.SourceId)
                .Union(selectedElectives.Select(s => s.LessonSourceId))
                .ToArray());

        var entries = await _lessonEntryRepository.GetBySourceIdsAsync(sources.Select(x => x.Id).ToArray());

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

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}