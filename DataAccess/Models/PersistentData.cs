namespace DataAccess.Models;

public class PersistentData
{
    public int Id { get; set; }
    public Dictionary<string, DateTimeOffset> NextScheduleParseDateTime { get; set; } = [];
}