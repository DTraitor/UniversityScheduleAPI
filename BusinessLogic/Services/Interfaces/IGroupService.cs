using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<ICollection<string>> GetUserGroups(long telegramId);
    Task<bool> GroupExists(string groupName);
    Task<Result<ICollection<int>>> GetSubgroups(long telegramId);
}