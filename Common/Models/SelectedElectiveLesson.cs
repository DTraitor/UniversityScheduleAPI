namespace Common.Models;

public class SelectedElectiveLesson
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonSourceId { get; set; }
    public string LessonName { get; set; }
    public string? LessonType { get; set; }
    public int SubgroupNumber { get; set; }
}