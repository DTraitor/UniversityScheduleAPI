namespace BusinessLogic.DTO;

public record CreateElectiveLessonDto
{
    public long TelegramId { get; init; }
    public int ElectiveLessonId { get; init; }
}