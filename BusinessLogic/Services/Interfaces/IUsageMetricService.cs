using DataAccess.Models;

namespace BusinessLogic.Services.Interfaces;

public interface IUsageMetricService
{
    void AddUsage(DateTimeOffset timeStamp, DateTimeOffset lookUpDate, int userId);
    IEnumerable<UsageMetric> GetUsages();
    int Count();
}