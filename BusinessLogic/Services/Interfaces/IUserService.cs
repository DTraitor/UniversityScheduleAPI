using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IUserService
{
    Task<bool> UserExists(long telegramId);
    Task<UserDtoOutput> CreateUser(UserDtoInput user);
    Task<UserDtoOutput> ChangeGroup(UserDtoInput user);
}