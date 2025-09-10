namespace DataAccess.Models;

public class ElectedLesson
{
    public int Id { get; set; }
    public int UserId { get; set; }
    // Name of the elected lesson
    public string Name { get; set; }
    public int ElectiveLessonDayId { get; set; }
    public int ElectiveLessonId { get; set; }
}