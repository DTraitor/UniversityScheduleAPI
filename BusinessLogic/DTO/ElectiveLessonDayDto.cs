namespace BusinessLogic.DTO;

public class ElectiveLessonDayDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
}