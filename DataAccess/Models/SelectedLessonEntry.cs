using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class SelectedLessonEntry : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SourceId { get; set; }
    public int EntryId { get; set; }

    //Alert related things
    public string EntryName { get; set; }
    public string? Type { get; set; }
    public bool WeekNumber { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
}