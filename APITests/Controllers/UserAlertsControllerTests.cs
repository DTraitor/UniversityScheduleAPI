using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers;
using BusinessLogic.Services.Interfaces;
using Common.Models;
using Common.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace APITests.Controllers;

[TestFixture]
public class UserAlertsControllerTests
{
    private Mock<IUserAlertService> _service = null!;
    private Mock<ILogger<UserAlertsController>> _logger = null!;
    private UserAlertsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IUserAlertService>();
        _logger = new Mock<ILogger<UserAlertsController>>();
        _controller = new UserAlertsController(_service.Object, _logger.Object);
    }

    [Test]
    public async Task GetAlerts_Returns_Ok_With_Mapped_DTOs()
    {
        var alerts = new List<(UserAlert, User)>
        {
            (new UserAlert { Id = 1, AlertType = UserAlertType.GroupRemoved, Options = new Dictionary<string,string>() }, new User { TelegramId = 123 })
        };

        _service.Setup(x => x.GetAlerts(5)).ReturnsAsync(alerts);

        var result = await _controller.GetAlerts(5);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var arr = ((IEnumerable<object>)ok.Value!).ToArray();
        Assert.That(arr.Length, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteProcessedAlerts_Calls_Service_And_Returns_Ok()
    {
        var ids = new List<int> { 1, 2 };

        _service.Setup(x => x.RemoveProcessedAlerts(ids)).Returns(Task.CompletedTask).Verifiable();

        var result = await _controller.DeleteProcessedAlerts(ids);

        Assert.That(result, Is.TypeOf<OkResult>());
        _service.Verify();
    }
}
