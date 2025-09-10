namespace DataAccess.Models;

public class GroupLesson
{
    public int Id { get; set; }
    public int GroupId { get; set; }

    public string Title { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public IEnumerable<string> Teacher { get; set; } = [];

    public TimeOnly StartTime { get; set; }
    public TimeSpan Length { get; set; }

    // false - first week
    // true - second week
    public bool Week { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}