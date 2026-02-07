using Common.Enums;
using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DataAccessTests.Repositories;

[TestFixture]
public class UserAlertRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UserAlertRepository>> _loggerMock;
    private UserAlertRepository _repository;
    private SqliteConnection _connection;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ScheduleDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<UserAlertRepository>>();
        _repository = new UserAlertRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddEntityToContext()
    {
        // Arrange
        var alert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} };

        // Act
        _repository.Add(alert);
        _context.SaveChanges();

        // Assert
        var result = _context.UserAlerts.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AlertType, Is.EqualTo(UserAlertType.None));
        Assert.That(result.OptionsJson, Is.EqualTo("{\"test\":\"something\"}"));
    }

    [Test]
    public void Update_ShouldUpdateEntityInContext()
    {
        // Arrange
        var alert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} };
        _context.UserAlerts.Add(alert);
        _context.SaveChanges();
        _context.Entry(alert).State = EntityState.Detached;

        var updatedAlert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} };

        // Act
        _repository.Update(updatedAlert);
        _context.SaveChanges();

        // Assert
        var result = _context.UserAlerts.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AlertType, Is.EqualTo(UserAlertType.None));
        Assert.That(result.OptionsJson, Is.EqualTo("{\"test\":\"something\"}"));
    }

    [Test]
    public void Delete_ShouldRemoveEntityFromContext()
    {
        // Arrange
        var alert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} };
        _context.UserAlerts.Add(alert);
        _context.SaveChanges();

        // Act
        _repository.Delete(alert);
        _context.SaveChanges();

        // Assert
        var result = _context.UserAlerts.Find(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var alert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} };
        _context.UserAlerts.Add(alert);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.AlertType, Is.EqualTo(UserAlertType.None));
        Assert.That(result.OptionsJson, Is.EqualTo("{\"test\":\"something\"}"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var alerts = new List<UserAlert>
        {
            new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test", "something"}} },
            new UserAlert { Id = 2, UserId = 2, AlertType = UserAlertType.EntryRemoved, Options = new(){{"test1", "something"}} },
            new UserAlert { Id = 3, UserId = 1, AlertType = UserAlertType.GroupRemoved, Options = new(){{"test2", "something"}} },
        };
        _context.UserAlerts.AddRange(alerts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetAllLimitAsync_ShouldReturnLimitedEntities()
    {
        // Arrange
        var alerts = new List<UserAlert>
        {
            new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test1", "something"}} },
            new UserAlert { Id = 2, UserId = 2, AlertType = UserAlertType.EntryRemoved, Options = new(){{"test2", "something"}} },
            new UserAlert { Id = 3, UserId = 1, AlertType = UserAlertType.GroupRemoved, Options = new(){{"test3", "something"}} },
            new UserAlert { Id = 4, UserId = 2, AlertType = UserAlertType.SourceRemoved, Options = new(){{"test4", "something"}} },
            new UserAlert { Id = 5, UserId = 1, AlertType = UserAlertType.None, Options = new(){{"test5", "something"}} },
        };
        _context.UserAlerts.AddRange(alerts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllLimitAsync(3);

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result.First().Id, Is.EqualTo(1));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var alert = new UserAlert { Id = 1, UserId = 1, AlertType = UserAlertType.EntryRemoved, Options = new(){{"test", "something"}} };
        _repository.Add(alert);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.UserAlerts.FindAsync(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AlertType, Is.EqualTo(UserAlertType.EntryRemoved));
        Assert.That(result.OptionsJson, Is.EqualTo("{\"test\":\"something\"}"));
    }
}
