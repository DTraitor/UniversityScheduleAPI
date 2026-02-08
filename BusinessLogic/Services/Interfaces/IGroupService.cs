using Common.Result;

namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<string>> GetUserGroups(long telegramId);
    Task<bool> GroupExists(string groupName);
    Task<Result<IEnumerable<int>>> GetSubgroups(long telegramId);
}