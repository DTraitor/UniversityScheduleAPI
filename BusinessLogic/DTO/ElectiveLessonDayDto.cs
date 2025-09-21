namespace BusinessLogic.DTO;

public class ElectiveLessonDayDto
{
    public int Id { get; set; }
    public int WeekNumber { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
}