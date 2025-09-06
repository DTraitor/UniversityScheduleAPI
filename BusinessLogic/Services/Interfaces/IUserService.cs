using BusinessLogic.DTO;

namespace BusinessLogic.Services.Interfaces;

public interface IUserService
{
    Task<UserDtoOutput> CreateUser(UserDtoInput user);
}