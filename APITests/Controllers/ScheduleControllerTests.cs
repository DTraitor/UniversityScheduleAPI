using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers;
using BusinessLogic.Services.Interfaces;
using Common.Models.Internal;
using Common.Result;
using Common.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace APITests.Controllers;

[TestFixture]
public class ScheduleControllerTests
{
    private Mock<IScheduleService> _scheduleService = null!;
    private Mock<ILogger<ScheduleController>> _logger = null!;
    private ScheduleController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _scheduleService = new Mock<IScheduleService>();
        _logger = new Mock<ILogger<ScheduleController>>();
        _controller = new ScheduleController(_scheduleService.Object, _logger.Object);
    }

    [Test]
    public async Task GetScheduleForDate_Returns_Ok_With_Lessons()
    {
        var lessons = new List<UserLesson>
        {
            new UserLesson { Title = "T1", BeginTime = TimeSpan.FromHours(9), Duration = TimeSpan.FromHours(1), Teacher = new List<string>{"A"} }
        };

        _scheduleService.Setup(x => x.GetScheduleForDate(It.IsAny<DateTimeOffset>(), 123L)).ReturnsAsync(new Result<ICollection<UserLesson>, (DateTimeOffset, DateTimeOffset)>(lessons));

        var result = await _controller.GetScheduleForDate(DateTimeOffset.UtcNow, 123);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetScheduleForDate_Returns_NotFound_When_UserNotFound()
    {
        _scheduleService.Setup(x => x.GetScheduleForDate(It.IsAny<DateTimeOffset>(), 123L)).ReturnsAsync(ErrorType.UserNotFound);

        var result = await _controller.GetScheduleForDate(DateTimeOffset.UtcNow, 123);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetScheduleForDate_Returns_BadRequest_When_OutOfRange()
    {
        var start = DateTimeOffset.UtcNow.Date;
        var end = start.AddDays(1);
        _scheduleService.Setup(x => x.GetScheduleForDate(It.IsAny<DateTimeOffset>(), 123L)).ReturnsAsync(((ErrorType.TimetableDateOutOfRange, (start, end))));

        var result = await _controller.GetScheduleForDate(DateTimeOffset.UtcNow, 123);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var bad = (BadRequestObjectResult)result;
        Assert.That(bad.Value, Is.Not.Null);
    }
}
