using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public int? GroupId { get; set; }
    // TODO: Encrypt this
    [Length(0,512)]
    public string? AccessToken { get; set; }
}