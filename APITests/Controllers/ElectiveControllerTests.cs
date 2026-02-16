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
public class ElectiveControllerTests
{
    private Mock<IElectiveService> _electiveService = null!;
    private Mock<ILogger<ElectiveController>> _logger = null!;
    private ElectiveController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _electiveService = new Mock<IElectiveService>();
        _logger = new Mock<ILogger<ElectiveController>>();
        _controller = new ElectiveController(_electiveService.Object, _logger.Object);
    }

    [Test]
    public async Task GetLevels_Returns_Ok_With_Mapped_Levels()
    {
        var sources = new List<LessonSource>
        {
            new LessonSource { Id = 1, Name = "Lvl1" },
            new LessonSource { Id = 2, Name = "Lvl2" }
        };

        _electiveService.Setup(x => x.GetPossibleLevelsAsync()).ReturnsAsync(sources);

        var result = await _controller.GetLevels();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var ok = (OkObjectResult)result;
        var items = ((IEnumerable<object>)ok.Value!).ToArray();
        Assert.That(items.Length, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLessons_Returns_Ok_When_Service_Succeeds()
    {
        var lessons = new List<string> { "A", "B" };
        _electiveService.Setup(x => x.GetLessonsByNameAsync("a", 1)).ReturnsAsync(new Result<ICollection<string>>(lessons));

        var result = await _controller.GetLessons("a", 1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetLessons_Returns_BadRequest_When_TooManyElements()
    {
        _electiveService.Setup(x => x.GetLessonsByNameAsync("a", 1)).ReturnsAsync(ErrorType.TooManyElements);

        var result = await _controller.GetLessons("a", 1);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
