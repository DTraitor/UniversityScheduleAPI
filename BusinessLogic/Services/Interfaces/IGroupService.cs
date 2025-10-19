namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<bool> GroupExists(string groupName);
    Task<IEnumerable<int>> GetSubgroups(long telegramId);
}