namespace BusinessLogic.DTO;

public record LessonDto
{
    public string Title { get; set; }
    public string? LessonType { get; set; }
    public string? Teacher { get; set; }
    public string? Location { get; set; }
    public bool Cancelled { get; set; }

    public TimeOnly BeginTime { get; set; }
    public TimeSpan Duration { get; set; }
}