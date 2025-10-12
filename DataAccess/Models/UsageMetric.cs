namespace DataAccess.Models;

public class UsageMetric
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}