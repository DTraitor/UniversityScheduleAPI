namespace BusinessLogic.DTO;

public record ElectiveLessonDto
{
    public int Id { get; set; }

    public string Title { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public IEnumerable<string> Teacher { get; set; } = [];

    public int WeekNumber { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Length { get; set; }
}