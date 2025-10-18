using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class UserAlert : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserAlertType AlertType { get; set; }
    public Dictionary<string, string> Options { get; set; }
}