using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class User : IEntityId
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}