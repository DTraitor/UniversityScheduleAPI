using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Controllers;
using BusinessLogic.Services.Interfaces;
using Common.Result;
using Common.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace APITests.Controllers;

[TestFixture]
public class GroupControllerTests
{
    private Mock<IGroupService> _groupService = null!;
    private Mock<ILogger<GroupController>> _logger = null!;
    private GroupController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _groupService = new Mock<IGroupService>();
        _logger = new Mock<ILogger<GroupController>>();
        _controller = new GroupController(_groupService.Object, _logger.Object);
    }

    [Test]
    public async Task GetUserGroups_Returns_Ok_With_List()
    {
        _groupService.Setup(x => x.GetUserGroups(123)).ReturnsAsync(new List<string> { "G1", "G2" } as ICollection<string>);

        var result = await _controller.GetUserGroups(123);
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Exists_Returns_Ok_With_Bool()
    {
        _groupService.Setup(x => x.GroupExists("grp")).ReturnsAsync(true);

        var result = await _controller.Exists("grp");
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        Assert.That(ok.Value, Is.True);
    }

    [Test]
    public async Task GetSubgroups_Returns_NotFound_When_UserNotFound()
    {
        _groupService.Setup(x => x.GetSubgroups(123)).ReturnsAsync(ErrorType.UserNotFound);

        var result = await _controller.GetSubgroups(123);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetSubgroups_Returns_Ok_When_Success()
    {
        _groupService.Setup(x => x.GetSubgroups(123)).ReturnsAsync(new Result<ICollection<int>>(new List<int> { 1, 2 }));

        var result = await _controller.GetSubgroups(123);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }
}
