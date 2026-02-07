using Common.Enums;
using Common.Models.Interface;

namespace Common.Models.Internal;

public class UserLesson : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string Title { get; set; }
    public string? LessonType { get; set; }
    public IEnumerable<string> Teacher { get; set; } = null;
    public string? Location { get; set; }
    public bool Cancelled { get; set; }

    public TimeSpan BeginTime { get; set; }
    public TimeSpan Duration { get; set; }

    public RepeatType RepeatType { get; set; }
    // If RepeatType is set to daily, will repeat each RepeatCount days
    public int RepeatCount { get; set; }

    public SelectedLessonSourceType SelectedLessonSourceType { get; set; } = SelectedLessonSourceType.None;
    public int LessonSourceId { get; set; }

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string TimeZoneId { get; set; } = "UTC";

    public DateTimeOffset? OccurrencesCalculatedTill { get; set; }
}