using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class UserModified : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
}