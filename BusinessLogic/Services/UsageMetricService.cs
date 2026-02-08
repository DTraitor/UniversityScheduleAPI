using BusinessLogic.Services.Interfaces;
using Common.Models;

namespace BusinessLogic.Services;

public class UsageMetricService : IUsageMetricService
{
    public List<UsageMetric> CurrentUsages { get; set; } = new();

    public void AddUsage(DateTimeOffset timeStamp, DateTimeOffset lookUpDate, int userId)
    {
        CurrentUsages.Add(new UsageMetric()
        {
            Timestamp = timeStamp,
            ScheduleTime = lookUpDate,
            UserId = userId,
        });
    }

    public ICollection<UsageMetric> GetUsages()
    {
        var curr = CurrentUsages.ToList();
        CurrentUsages.Clear();
        return curr;
    }

    public int Count()
    {
        return CurrentUsages.Count;
    }
}