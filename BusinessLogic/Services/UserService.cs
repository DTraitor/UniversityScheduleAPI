using BusinessLogic.DTO;
using BusinessLogic.Enums;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _logger = logger;
    }

    public async Task<bool> UserExists(long telegramId)
    {
        return (await _userRepository.GetByTelegramIdAsync(telegramId)) != null;
    }

    public async Task<UserDtoOutput> CreateUser(UserDtoInput userData)
    {
        var group = await _groupRepository.GetByNameAsync(userData.GroupName);
        if (group == null)
            throw new KeyNotFoundException("No group with such name found.");

        var user = await _userRepository.GetByTelegramIdAsync(userData.TelegramId);
        if (user != null)
        {
            user.GroupId = group.Id;
            user = _userRepository.Update(user);
        }
        else
        {
            user = await _userRepository.AddAsync(new User
            {
                TelegramId = userData.TelegramId,
                GroupId = group.Id,
            });
        }

        await _userRepository.SaveChangesAsync();

        return new UserDtoOutput
        {
            Id = user.Id,
            GroupId = user.GroupId,
            ElectiveStatus = ElectiveStatusEnum.None,
        };
    }

    public async Task<UserDtoOutput> ChangeGroup(UserDtoInput userData)
    {
        var user = await _userRepository.GetByTelegramIdAsync(userData.TelegramId);
        if (user == null)
            throw new KeyNotFoundException("No user with such telegram id.");

        var group = await _groupRepository.GetByNameAsync(userData.GroupName);
        if (group == null)
            throw new KeyNotFoundException("No group with such name found.");

        user.GroupId = group.Id;
        var result = _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new UserDtoOutput
        {
            Id = result.Id,
            GroupId = result.GroupId,
            ElectiveStatus = ElectiveStatusEnum.None,
        };
    }
}