using BusinessLogic.Services;
using Common.Models.Internal;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Moq;

namespace BusinessLogicTests.Services;

[TestFixture]
public class ScheduleServiceTests
{
    private Mock<IUserRepository> _userRepo;
    private Mock<IUserLessonRepository> _userLessonRepo;
    private Mock<IUserLessonOccurenceRepository> _occRepo;
    private Mock<BusinessLogic.Services.Interfaces.IUsageMetricService> _usageMetricService;
    private Mock<Microsoft.Extensions.Logging.ILogger<ScheduleService>> _logger;
    private ScheduleService _service;

    [SetUp]
    public void SetUp()
    {
        _userRepo = new Mock<IUserRepository>();
        _userLessonRepo = new Mock<IUserLessonRepository>();
        _occRepo = new Mock<IUserLessonOccurenceRepository>();
        _usageMetricService = new Mock<BusinessLogic.Services.Interfaces.IUsageMetricService>();
        _logger = new Mock<Microsoft.Extensions.Logging.ILogger<ScheduleService>>();

        _service = new ScheduleService(_userRepo.Object, _userLessonRepo.Object, _occRepo.Object, _usageMetricService.Object, _logger.Object);
    }

    [Test]
    public async Task GetScheduleForDate_ReturnsLessons_And_RecordsUsage()
    {
        var user = new User { Id = 42, TelegramId = 11111 };
        _userRepo.Setup(x => x.GetByTelegramIdAsync(11111, CancellationToken.None)).ReturnsAsync(user);

        var date = DateTimeOffset.Parse("2026-03-01T10:00:00+02:00").Date;
        var begin = date.ToUniversalTime();

        var occs = new List<UserLessonOccurrence> { new UserLessonOccurrence { LessonId = 1 } };
        _occRepo.Setup(x => x.GetByUserIdAndBetweenDateAsync(user.Id, begin, begin.AddDays(1))).ReturnsAsync(occs);

        var lessons = new List<UserLesson> { new UserLesson { Id = 1, UserId = user.Id } };
        _userLessonRepo.Setup(x => x.GetByIdsAsync(It.Is<int[]>(a => a.SequenceEqual(new[] { 1 })))).ReturnsAsync(lessons);

        var res = await _service.GetScheduleForDate(DateTimeOffset.Parse("2026-03-01T10:00:00+02:00"), 11111);

        Assert.That(res.IsSuccess, Is.True);
        Assert.That(res.Value.Count, Is.EqualTo(1));
        _usageMetricService.Verify(x => x.AddUsage(It.IsAny<DateTimeOffset>(), begin, user.Id), Times.Once);
    }

    [Test]
    public async Task GetScheduleForDate_ReturnsUserNotFound_WhenUserMissing()
    {
        _userRepo.Setup(x => x.GetByTelegramIdAsync(99999, CancellationToken.None)).ReturnsAsync((User?)null);

        var res = await _service.GetScheduleForDate(DateTimeOffset.UtcNow, 99999);

        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(Common.Enums.ErrorType.UserNotFound));
    }

    [Test]
    public async Task GetScheduleForDate_ReturnsOutOfRange_WhenDateOutsideLimits()
    {
        var user = new User { Id = 50, TelegramId = 555 };
        _userRepo.Setup(x => x.GetByTelegramIdAsync(555, CancellationToken.None)).ReturnsAsync(user);

        var date = DateTimeOffset.Parse("2024-01-01T00:00:00+02:00");
        var res = await _service.GetScheduleForDate(date, 555);

        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(Common.Enums.ErrorType.TimetableDateOutOfRange));
    }
}
