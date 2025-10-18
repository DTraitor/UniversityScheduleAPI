using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class LessonEntry : IEntityId
{
    public int Id { get; set; }
    public int SourceId { get; set; }

    public string Title { get; set; }
    public string? Type { get; set; }
    public string? Location { get; set; }
    public int SubGroupNumber { get; set; }
    public IEnumerable<string> Teachers { get; set; } = [];

    public TimeSpan StartTime { get; set; }
    public TimeSpan Length { get; set; }

    // false - first week
    // true - second week
    public bool Week { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}