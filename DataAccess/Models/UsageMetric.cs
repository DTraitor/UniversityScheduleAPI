namespace DataAccess.Models;

public class UsageMetric
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset ScheduleTime { get; set; }
}