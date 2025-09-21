namespace BusinessLogic.Configuration;

public class GroupScheduleParsingOptions
{
    public string ScheduleUrl { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
}