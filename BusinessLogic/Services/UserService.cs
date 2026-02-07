using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly IUserModifiedRepository _userModifiedRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserModifiedRepository userModifiedRepository,
        IUserRepository userRepository,
        ILessonSourceRepository lessonSourceRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ILogger<UserService> logger)
    {
        _userModifiedRepository = userModifiedRepository;
        _userRepository = userRepository;
        _lessonSourceRepository = lessonSourceRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        _logger = logger;
    }

    public async Task<bool> UserExists(long telegramId)
    {
        return (await _userRepository.GetByTelegramIdAsync(telegramId)) != null;
    }

    public async Task<UserDtoOutput> CreateUser(UserDtoInput userData)
    {
        var group = await _lessonSourceRepository.GetByNameAndSourceTypeAsync(userData.GroupName, LessonSourceType.Group);
        if (group == null)
            throw new KeyNotFoundException("No group with such name found.");

        var user = await _userRepository.GetByTelegramIdAsync(userData.TelegramId);
        if (user == null)
        {
            user = new User
            {
                TelegramId = userData.TelegramId,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            _userRepository.Add(user);
        }

        await UpdateUserGroup(user, userData);

        await _userRepository.SaveChangesAsync();

        return new UserDtoOutput
        {
            Id = user.Id,
        };
    }

    public async Task<UserDtoOutput> ChangeGroup(UserDtoInput userData)
    {
        var user = await _userRepository.GetByTelegramIdAsync(userData.TelegramId);
        if (user == null)
            throw new KeyNotFoundException("No user with such telegram id.");

        await UpdateUserGroup(user, userData);

        return new UserDtoOutput
        {
            Id = user.Id,
        };
    }

    private async Task UpdateUserGroup(User user, UserDtoInput userData)
    {
        var group = await _lessonSourceRepository.GetByNameAndLimitAsync(userData.GroupName, 5);
        if (group == null)
            throw new KeyNotFoundException("No group with such name found.");

        var selectedGroup
            = (await _selectedLessonSourceRepository.GetByUserId(user.Id)).FirstOrDefault();

        if (selectedGroup == null)
        {
            selectedGroup = new SelectedLessonSource()
            {
                UserId = user.Id,
                SourceId = group.Id,
                SubGroupNumber = userData.SubGroup,
                LessonSourceType = LessonSourceType.Group,
                SourceName = group.Name,
            };

            _selectedLessonSourceRepository.Add(selectedGroup);
            _userModifiedRepository.Add(user.Id);

            await _selectedLessonSourceRepository.SaveChangesAsync();
            await _userModifiedRepository.SaveChangesAsync();

            return;
        }

        selectedGroup.SourceId = group.Id;
        selectedGroup.SubGroupNumber = userData.SubGroup;
        selectedGroup.SourceName = group.Name;

        _selectedLessonSourceRepository.Update(selectedGroup);
        _userModifiedRepository.Add(user.Id);

        await _selectedLessonSourceRepository.SaveChangesAsync();
        await _userModifiedRepository.SaveChangesAsync();
    }
}