using BusinessLogic.Services.Interfaces;
using DataAccess.Enums;
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

    public async Task<bool> GroupExists(string groupName)
    {
        return (await _lessonSourceRepository.GetByNameAndSourceTypeAsync(groupName, LessonSourceType.Group)) != null;
    }

    public async Task<IEnumerable<int>> GetSubgroups(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        var selected = (await _selectedLessonSourceRepository.GetByUserIdAndSourceType(user.Id, LessonSourceType.Group)).FirstOrDefault();

        if (selected == null)
            throw new InvalidOperationException("User has no group");

        var group = await _lessonSourceRepository.GetByIdAsync(selected.SourceId);

        var entries = await _lessonEntryRepository.GetBySourceIdAsync(group.Id);

        return entries.Select(x => x.SubGroupNumber).Distinct().ToArray();
    }
}