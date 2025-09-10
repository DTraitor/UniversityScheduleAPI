namespace DataAccess.Models;

public class ElectiveLesson
{
    public int Id { get; set; }
    public int ElectiveLessonDayId { get; set; }

    public string Title { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public IEnumerable<string> Teacher { get; set; } = [];

    public TimeOnly StartTime { get; set; }
    public TimeSpan Length { get; set; }
}