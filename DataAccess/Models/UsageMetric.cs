using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class UsageMetric : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public DateTimeOffset ScheduleTime { get; set; }
}