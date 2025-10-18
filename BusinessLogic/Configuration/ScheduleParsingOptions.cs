namespace BusinessLogic.Configuration;

public class ScheduleParsingOptions
{
    public string ScheduleUrl { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}