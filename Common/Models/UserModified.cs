using Common.Models.Interface;

namespace Common.Models;

public class UserModified : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
}