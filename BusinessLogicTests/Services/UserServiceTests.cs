using Moq;
using BusinessLogic.Services;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Common.Models;
using Common.Enums;

namespace BusinessLogicTests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserModifiedRepository> _userModifiedRepository = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<ILessonSourceRepository> _lessonSourceRepository = null!;
    private Mock<ISelectedLessonSourceRepository> _selectedLessonSourceRepository = null!;
    private Mock<ISelectedElectiveLessonRepository> _selectedElectiveLessonRepository = null!;
    private Mock<ILogger<UserService>> _logger = null!;

    private UserService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userModifiedRepository = new Mock<IUserModifiedRepository>();
        _userRepository = new Mock<IUserRepository>();
        _lessonSourceRepository = new Mock<ILessonSourceRepository>();
        _selectedLessonSourceRepository = new Mock<ISelectedLessonSourceRepository>();
        _selectedElectiveLessonRepository = new Mock<ISelectedElectiveLessonRepository>();
        _logger = new Mock<ILogger<UserService>>();

        _sut = new UserService(
            _userModifiedRepository.Object,
            _userRepository.Object,
            _lessonSourceRepository.Object,
            _selectedLessonSourceRepository.Object,
            _selectedElectiveLessonRepository.Object,
            _logger.Object);
    }

    [Test]
    public async Task CreateUserAsync_CallsRepositoryAddAndSave()
    {
        var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _userRepository.Setup(r => r.BeginTransactionAsync(CancellationToken.None)).ReturnsAsync(transactionMock.Object);

        await _sut.CreateUserAsync(12345);

        _userRepository.Verify(r => r.Add(It.Is<User>(u => u.TelegramId == 12345)), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
        transactionMock.Verify(t => t.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ChangeGroupAsync_ReturnsUserNotFound_WhenUserMissing()
    {
        _userRepository.Setup(r => r.GetByTelegramIdAsync(It.IsAny<long>(), CancellationToken.None)).ReturnsAsync((User?)null);

        var result = await _sut.ChangeGroupAsync(9999, "group", 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ErrorType.UserNotFound));
    }

    [Test]
    public async Task ChangeGroupAsync_ReturnsGroupNotFound_WhenGroupMissingOrNotGroupType()
    {
        var user = new User { Id = 1, TelegramId = 111 };
        _userRepository.Setup(r => r.GetByTelegramIdAsync(It.IsAny<long>(), CancellationToken.None)).ReturnsAsync(user);

        // lesson source repo returns either empty or non-group
        _lessonSourceRepository.Setup(r => r.GetByNameAndLimitAsync(It.IsAny<string>(), 2)).ReturnsAsync(new List<Common.Models.LessonSource>());

        var result = await _sut.ChangeGroupAsync(111, "g", 1);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo(ErrorType.GroupNotFound));
    }
}
