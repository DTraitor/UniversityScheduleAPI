namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<IEnumerable<string>> GetUserGroups(long telegramId);
    Task<bool> GroupExists(string groupName);
    Task<IEnumerable<int>> GetSubgroups(long telegramId);
}