namespace DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public int? GroupId { get; set; }
    public long TelegramId { get; set; }
    // TODO: Encrypt this
    public string AccessToken { get; set; }
}