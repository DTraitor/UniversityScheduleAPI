namespace DataAccess.Models;

public class ScheduleLesson
{
    public int Id { get; set; }
    public int GroupId { get; set; }

    public string Title { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public string? Teacher { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeSpan Length { get; set; }

    public bool Week { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}