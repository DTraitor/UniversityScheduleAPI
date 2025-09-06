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

    public async Task<UserDtoOutput> CreateUser(UserDtoInput user)
    {
        var result = await _userRepository.AddAsync(new User
        {
            TelegramId = user.TelegramId,
            GroupId = (await _groupRepository.GetByNameAsync(user.GroupName)).Id,
        });
        await _userRepository.SaveChangesAsync();

        return new UserDtoOutput
        {
            Id = result.Id,
            GroupId = result.GroupId,
            ElectiveStatus = ElectiveStatusEnum.None,
        };
    }
}