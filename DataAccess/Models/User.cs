namespace DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public int? GroupId { get; set; }
    public string GroupName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}