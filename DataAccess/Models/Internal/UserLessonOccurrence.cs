namespace DataAccess.Models.Internal;

public class UserLessonOccurrence
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int UserId { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}