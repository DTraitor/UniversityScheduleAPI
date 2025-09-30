using DataAccess.Enums;

namespace DataAccess.Models;

public class UserAlert
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserAlertType AlertType { get; set; }
    public Dictionary<string, string> Options { get; set; }
}