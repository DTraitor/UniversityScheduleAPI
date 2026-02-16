using BusinessLogic.Services;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace BusinessLogicTests.Services;

[TestFixture]
public class GroupServiceTests
{
    private Mock<ILessonSourceRepository> _lessonSourceRepo;
    private Mock<ISelectedLessonSourceRepository> _selectedLessonSourceRepo;
    private Mock<IUserRepository> _userRepo;
    private Mock<ILessonEntryRepository> _lessonEntryRepo;
    private Mock<ILogger<GroupService>> _logger;
    private GroupService _service;

    [SetUp]
    public void SetUp()
    {
        _lessonSourceRepo = new Mock<ILessonSourceRepository>();
        _selectedLessonSourceRepo = new Mock<ISelectedLessonSourceRepository>();
        _userRepo = new Mock<IUserRepository>();
        _lessonEntryRepo = new Mock<ILessonEntryRepository>();
        _logger = new Mock<ILogger<GroupService>>();

        _service = new GroupService(_lessonSourceRepo.Object, _selectedLessonSourceRepo.Object, _userRepo.Object, _lessonEntryRepo.Object, _logger.Object);
    }

    [Test]
    public async Task GetUserGroups_Returns_SourceNames_WhenUserExists()
    {
        var user = new User { Id = 1, TelegramId = 12345 };
        _userRepo.Setup(x => x.GetByTelegramIdAsync(12345, CancellationToken.None)).ReturnsAsync(user);

        var selected = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { SourceName = "Group A" },
            new SelectedLessonSource { SourceName = "Group B" }
        };

        _selectedLessonSourceRepo.Setup(x => x.GetByUserId(user.Id)).ReturnsAsync(selected);

        var result = await _service.GetUserGroups(12345);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Is.EquivalentTo(new[] { "Group A", "Group B" }));
    }

    [Test]
    public async Task GetUserGroups_Returns_Empty_WhenUserNotFound()
    {
        _userRepo.Setup(x => x.GetByTelegramIdAsync(99999, CancellationToken.None)).ReturnsAsync((User?)null);

        var result = await _service.GetUserGroups(99999);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GroupExists_ReturnsTrue_WhenExactlyOne()
    {
        _lessonSourceRepo.Setup(x => x.GetByNameAndLimitAsync("group a", 2)).ReturnsAsync(new List<LessonSource> { new LessonSource { Id = 1 } });

        var result = await _service.GroupExists("group a");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task GetSubgroups_ReturnsDistinctSubgroups_WhenUserAndGroupFound()
    {
        var user = new User { Id = 2, TelegramId = 222 };
        _userRepo.Setup(x => x.GetByTelegramIdAsync(222, CancellationToken.None)).ReturnsAsync(user);

        var selected = new List<SelectedLessonSource> { new SelectedLessonSource { SourceId = 10 } };
        _selectedLessonSourceRepo.Setup(x => x.GetByUserId(user.Id)).ReturnsAsync(selected);

        var group = new LessonSource { Id = 10 };
        _lessonSourceRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(group);

        var entries = new List<LessonEntry>
        {
            new LessonEntry { SubGroupNumber = 0 },
            new LessonEntry { SubGroupNumber = 1 },
            new LessonEntry { SubGroupNumber = 1 },
            new LessonEntry { SubGroupNumber = -1 }
        };

        _lessonEntryRepo.Setup(x => x.GetBySourceIdAsync(group.Id)).ReturnsAsync(entries);

        var res = await _service.GetSubgroups(222);

        Assert.That(res.IsSuccess, Is.True);
        var arr = res.Value.ToArray();
        Assert.That(arr, Is.EquivalentTo(new[] { 0, 1, -1 }));
    }

    [Test]
    public async Task GetSubgroups_ReturnsUserNotFound_WhenUserMissing()
    {
        _userRepo.Setup(x => x.GetByTelegramIdAsync(11, CancellationToken.None)).ReturnsAsync((User?)null);

        var res = await _service.GetSubgroups(11);

        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(Common.Enums.ErrorType.UserNotFound));
    }
}
