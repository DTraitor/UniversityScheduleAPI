using BusinessLogic.Services;
using Common.Enums;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogicTests.Services;

[TestFixture]
public class UserAlertServiceTests
{
    private Mock<IUserAlertRepository> _userAlertRepoMock;
    private Mock<IUserRepository> _userRepoMock;
    private Mock<ILogger<UserAlertService>> _loggerMock;
    private IServiceProvider _serviceProvider;
    private UserAlertService _service;

    [SetUp]
    public void SetUp()
    {
        _userAlertRepoMock = new Mock<IUserAlertRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserAlertService>>();

        // Build a real service provider which will supply our repository mocks when a scope is created
        var services = new ServiceCollection();
        services.AddSingleton<IUserAlertRepository>(_ => _userAlertRepoMock.Object);
        services.AddSingleton<IUserRepository>(_ => _userRepoMock.Object);
        _serviceProvider = services.BuildServiceProvider();

        _service = new UserAlertService(_serviceProvider, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (_serviceProvider is IDisposable d) d.Dispose();
    }

    [Test]
    public void CreateUserAlert_Adds_ToCache()
    {
        _service.CreateUserAlert(5, UserAlertType.GroupRemoved, new Dictionary<string, string>{{"LessonName","A"}});

        var cached = _service.GetCachedAlerts();
        Assert.That(cached.Count, Is.EqualTo(1));
        Assert.That(cached.First().UserId, Is.EqualTo(5));
        Assert.That(cached.First().AlertType, Is.EqualTo(UserAlertType.GroupRemoved));
    }

    [Test]
    public async Task GetAlerts_JoinsAlertsWithUsers()
    {
        var alerts = new List<UserAlert>
        {
            new UserAlert{ Id = 1, UserId = 10, AlertType = UserAlertType.EntryRemoved, Options = new() },
            new UserAlert{ Id = 2, UserId = 20, AlertType = UserAlertType.GroupRemoved, Options = new() }
        };

        var users = new List<User>
        {
            new User{ Id = 10 },
            new User{ Id = 20 }
        };

        _userAlertRepoMock.Setup(x => x.GetAllLimitAsync(10)).ReturnsAsync(alerts);
        _userRepoMock.Setup(x => x.GetByIdsAsync(It.Is<int[]>(a => a.Length == 2), CancellationToken.None)).ReturnsAsync(users);

        var result = await _service.GetAlerts(10);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Select(r => r.Item2.Id), Is.EquivalentTo(alerts.Select(a => a.UserId)));
    }

    [Test]
    public async Task RemoveProcessedAlerts_CallsRepositoryMethods_And_CommitsTransaction()
    {
        var alertsToRemove = new List<int>{1,2,3};

        // Since IUserAlertRepository.BeginTransactionAsync returns IDbContextTransaction (which implements IAsyncDisposable),
        // we'll mock BeginTransactionAsync to return an object with CommitAsync method.
        var dbTransMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        dbTransMock.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);

        _userAlertRepoMock.Setup(x => x.BeginTransactionAsync(default)).ReturnsAsync(dbTransMock.Object);
        _userAlertRepoMock.Setup(x => x.RemoveByIdsAsync(alertsToRemove)).Returns(Task.CompletedTask).Verifiable();
        _userAlertRepoMock.Setup(x => x.SaveChangesAsync(default)).Returns(Task.CompletedTask).Verifiable();

        await _service.RemoveProcessedAlerts(alertsToRemove);

        _userAlertRepoMock.Verify(x => x.RemoveByIdsAsync(alertsToRemove), Times.Once);
        _userAlertRepoMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        dbTransMock.Verify(x => x.CommitAsync(default), Times.Once);
    }
}

// Helper interface to allow mocking IAsyncDisposable-like object in older frameworks
public interface IDisposableAsync : IAsyncDisposable, IDisposable { }

// Small concrete scope factory used by tests to provide a pre-built IServiceScope
internal class TestScopeFactory : IServiceScopeFactory
{
    private readonly IServiceScope _scope;
    public TestScopeFactory(IServiceScope scope) => _scope = scope;
    public IServiceScope CreateScope() => _scope;
}
