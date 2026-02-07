using Common.Models.Interface;

namespace Common.Models;

public class User : IEntityId
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}