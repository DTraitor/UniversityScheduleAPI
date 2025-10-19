namespace BusinessLogic.Services.Interfaces;

public interface IGroupService
{
    Task<bool> GroupExists(string groupName);
}