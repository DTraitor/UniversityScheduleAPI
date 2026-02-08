using Common.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUsageMetricService
{
    void AddUsage(DateTimeOffset timeStamp, DateTimeOffset lookUpDate, int userId);
    ICollection<UsageMetric> GetUsages();
}