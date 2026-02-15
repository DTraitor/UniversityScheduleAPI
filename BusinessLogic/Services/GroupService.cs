using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Result;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class GroupService : IGroupService
{
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILessonEntryRepository _lessonEntryRepository;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        ILessonSourceRepository lessonSourceRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        IUserRepository userRepository,
        ILessonEntryRepository lessonEntryRepository,
        ILogger<GroupService> logger)
    {
        _lessonSourceRepository = lessonSourceRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _userRepository = userRepository;
        _lessonEntryRepository = lessonEntryRepository;
        _logger = logger;
    }

    public async Task<ICollection<string>> GetUserGroups(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);

        if (user == null)
            return [];

        var selectedGroups = await _selectedLessonSourceRepository.GetByUserId(user.Id);

        return selectedGroups.Select(x => x.SourceName).ToList();
    }

    public async Task<bool> GroupExists(string groupName)
    {
        var groups = await _lessonSourceRepository.GetByNameAndLimitAsync(groupName.ToLower(), 2);
        return groups.Count == 1;
    }

    public async Task<Result<ICollection<int>>> GetSubgroups(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user == null)
            return ErrorType.UserNotFound;

        var selected = (await _selectedLessonSourceRepository.GetByUserId(user.Id)).FirstOrDefault();

        if (selected == null)
            return ErrorType.GroupNotFound;

        var group = await _lessonSourceRepository.GetByIdAsync(selected.SourceId);

        var entries = await _lessonEntryRepository.GetBySourceIdAsync(group.Id);

        return entries.Select(x => x.SubGroupNumber).Distinct().ToArray();
    }
}