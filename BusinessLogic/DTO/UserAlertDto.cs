using Common.Enums;

namespace BusinessLogic.DTO;

public record UserAlertDto
{
    public int Id { get; init; }
    public long UserTelegramId { get; init; }
    public UserAlertType AlertType { get; init; }
    public Dictionary<string, string> Options { get; init; }
}