namespace BusinessLogic.Configuration;

public class GroupScheduleParsingOptions
{
    public string ScheduleUrl { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}