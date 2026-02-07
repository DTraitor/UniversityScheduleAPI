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

public class UserLessonUpdaterService : IUserLessonUpdaterService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ILessonEntryRepository  _lessonEntryRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedElectiveLesson _selectedLessonEntryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<UserLessonUpdaterService> _logger;

    public UserLessonUpdaterService(
        ILessonSourceRepository lessonSourceRepository,
        ILessonEntryRepository lessonEntryRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedElectiveLesson selectedLessonEntryRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<UserLessonUpdaterService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _selectedLessonEntryRepository = selectedLessonEntryRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task ProcessModifiedUser(IEnumerable<UserModified> modifiedUsers)
    {
        var users = await _userRepository.GetByIdsAsync(modifiedUsers.Select(u => u.UserId));
        var usersIds = users.Select(u => u.Id).ToHashSet();
        var removed = _userLessonRepository.RemoveByUserIds(usersIds);
        _userLessonOccurenceRepository.ClearByLessonIds(removed);

        var selectedSources = await _selectedLessonSourceRepository.GetByUserIds(usersIds);
        var selectedEntries = await _selectedLessonEntryRepository.GetByUserIds(usersIds);

        Dictionary<int, List<SelectedLessonSource>> userIdToSelectedSources = selectedSources
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());
        Dictionary<int, List<SelectedLessonEntry>> userIdToSelectedEntries = selectedEntries
            .GroupBy(x => x.UserId)
            .ToDictionary(s => s.Key, s => s.Select(x => x).ToList());

        var sources = await _lessonSourceRepository.GetByIdsAsync(
            selectedSources
                .Select(s => s.SourceId)
                .Union(selectedEntries.Select(s => s.SourceId)));

        var entries = await _lessonEntryRepository.GetBySourceIdsAsync(sources.Select(x => x.Id));

        List<UserLesson> userLessons = new List<UserLesson>();

        foreach (var user in users)
        {
            if (userIdToSelectedSources.TryGetValue(user.Id, out var selectedSourcesDict))
            {
                foreach (var selectedSource in selectedSourcesDict)
                {
                    var source = sources.FirstOrDefault(x => x.Id == selectedSource.SourceId);

                    var lessonsToMap = entries
                        .Where(e => e.SourceId == source.Id);

                    if (selectedSource.SubGroupNumber != -1)
                        lessonsToMap = lessonsToMap
                            .Where(e => e.SubGroupNumber == selectedSource.SubGroupNumber || e.SubGroupNumber == -1);

                    if (selectedSource.Type != null)
                        lessonsToMap = lessonsToMap
                            .Where(e => e.Type == selectedSource.Type);

                    userLessons.AddRange(
                        ScheduleLessonsMapper.Map(
                            lessonsToMap.ToList(),
                            source.StartDate,
                            source.EndDate,
                            TimeZoneInfo.FindSystemTimeZoneById(source.TimeZone)
                        ).Select(x =>
                        {
                            x.UserId = user.Id;
                            x.SelectedLessonSourceType |= SelectedLessonSourceType.Source;
                            x.LessonSourceId = selectedSource.Id;
                            return x;
                        })
                    );
                }
            }

            if (userIdToSelectedEntries.TryGetValue(user.Id, out var selectedEntriesDict))
            {
                foreach (var selectedEntry in selectedEntriesDict.GroupBy(x => x.SourceId))
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
                            x.UserId = user.Id;
                            x.SelectedLessonSourceType |= SelectedLessonSourceType.Entry;
                            x.LessonSourceId = selectedEntry.Key;
                            return x;
                        })
                    );
                }
            }
        }

        _userLessonRepository.AddRange(userLessons);

        await _userLessonOccurenceRepository.SaveChangesAsync();
        await _userLessonRepository.SaveChangesAsync();
    }
}