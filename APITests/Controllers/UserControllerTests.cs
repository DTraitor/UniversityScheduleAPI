using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers;
using BusinessLogic.Services.Interfaces;
using Common.Models;
using Common.Result;
using Common.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace APITests.Controllers;

[TestFixture]
public class UserControllerTests
{
    private Mock<IUserService> _service = null!;
    private Mock<ILogger<UserController>> _logger = null!;
    private UserController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IUserService>();
        _logger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_service.Object, _logger.Object);
    }

    [Test]
    public async Task CreateUser_Calls_Service_And_Returns_Created()
    {
        _service.Setup(x => x.CreateUserAsync(123)).Returns(Task.CompletedTask).Verifiable();

        var result = await _controller.CreateUser(123);

        Assert.That(result, Is.TypeOf<CreatedResult>());
        _service.Verify();
    }

    [Test]
    public async Task ChangeGroup_Returns_Ok_On_Success()
    {
        _service.Setup(x => x.ChangeGroupAsync(123, "g", 1)).ReturnsAsync(Result.Success()).Verifiable();

        var result = await _controller.ChangeGroup(123, "g", 1);

        Assert.That(result, Is.TypeOf<OkResult>());
        _service.Verify();
    }

    [Test]
    public async Task ChangeGroup_Returns_NotFound_When_UserNotFound()
    {
        _service.Setup(x => x.ChangeGroupAsync(123, "g", 1)).ReturnsAsync(ErrorType.UserNotFound);

        var result = await _controller.ChangeGroup(123, "g", 1);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetUsersElective_Returns_Ok_When_Success()
    {
        var items = new List<SelectedElectiveLesson> { new SelectedElectiveLesson { Id = 1, LessonName = "L", SubgroupNumber = 2 } };
        _service.Setup(x => x.GetUserElectiveLessonAsync(123)).ReturnsAsync(new Result<ICollection<SelectedElectiveLesson>>(items));

        var result = await _controller.GetUsersElective(123);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task AddUsersElective_Returns_Ok_On_Success()
    {
        var input = new UserController.SelectedElectiveLessonInputOutput { Id = 1, LessonName = "L", SubgroupNumber = 1 };
        _service.Setup(x => x.AddUserElectiveLessonAsync(123, input.Id, input.LessonName, input.LessonType, input.SubgroupNumber)).ReturnsAsync(Result.Success()).Verifiable();

        var result = await _controller.AddUsersElective(123, input);

        Assert.That(result, Is.TypeOf<OkResult>());
        _service.Verify();
    }

    [Test]
    public async Task AddUsersElective_Returns_NotFound_When_Elective_NotFound()
    {
        var input = new UserController.SelectedElectiveLessonInputOutput { Id = 1, LessonName = "L", SubgroupNumber = 1 };
        _service.Setup(x => x.AddUserElectiveLessonAsync(123, input.Id, input.LessonName, input.LessonType, input.SubgroupNumber)).ReturnsAsync(ErrorType.NotFound);

        var result = await _controller.AddUsersElective(123, input);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task RemoveUsersElective_Returns_Ok_On_Success()
    {
        _service.Setup(x => x.RemoveUserElectiveLessonAsync(123, 1)).ReturnsAsync(Result.Success()).Verifiable();

        var result = await _controller.RemoveUsersElective(123, 1);

        Assert.That(result, Is.TypeOf<OkResult>());
        _service.Verify();
    }

    [Test]
    public async Task RemoveUsersElective_Returns_NotFound_When_NotFound()
    {
        _service.Setup(x => x.RemoveUserElectiveLessonAsync(123, 1)).ReturnsAsync(ErrorType.NotFound);

        var result = await _controller.RemoveUsersElective(123, 1);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }
}
