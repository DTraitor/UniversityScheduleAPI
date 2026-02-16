using BusinessLogic.Services;

namespace BusinessLogicTests.Services;

[TestFixture]
public class UsageMetricServiceTests
{
    private UsageMetricService _service;

    [SetUp]
    public void SetUp()
    {
        _service = new UsageMetricService();
    }

    [Test]
    public void AddUsage_AddsMetric_ToCurrentUsages()
    {
        var now = DateTimeOffset.UtcNow;
        _service.AddUsage(now, now.Date, 42);

        Assert.That(_service.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetUsages_ReturnsAndClears_CurrentUsages()
    {
        var now = DateTimeOffset.UtcNow;
        _service.AddUsage(now, now.Date, 1);
        _service.AddUsage(now.AddDays(1), now.AddDays(1).Date, 2);

        var usages = _service.GetUsages();
        Assert.That(usages.Count, Is.EqualTo(2));

        // After getting usages, internal list should be cleared
        Assert.That(_service.Count(), Is.EqualTo(0));
    }

    [Test]
    public void Count_ReflectsCurrentUsages()
    {
        Assert.That(_service.Count(), Is.EqualTo(0));
        _service.AddUsage(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.Date, 7);
        Assert.That(_service.Count(), Is.EqualTo(1));
    }
}
