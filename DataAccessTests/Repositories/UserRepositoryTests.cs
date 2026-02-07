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
public class UserRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UserRepository>> _loggerMock;
    private UserRepository _repository;
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

        _loggerMock = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddUserToContext()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };

        // Act
        _repository.Add(user);
        _context.SaveChanges();

        // Assert
        var result = _context.Users.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2024-02-20T01:12:31Z")));
        Assert.That(result.TelegramId, Is.EqualTo(12345));
    }

    [Test]
    public void Update_ShouldUpdateUserInContext()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };
        _context.Users.Add(user);
        _context.SaveChanges();
        _context.Entry(user).State = EntityState.Detached;

        var updatedUser = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2025-12-01T02:12:31Z")};

        // Act
        _repository.Update(updatedUser);
        _context.SaveChanges();

        // Assert
        var result = _context.Users.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2025-12-01T02:12:31Z")));
    }

    [Test]
    public void Delete_ShouldRemoveUserFromContext()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };
        _context.Users.Add(user);
        _context.SaveChanges();

        // Act
        _repository.Delete(user);
        _context.SaveChanges();

        // Assert
        var result = _context.Users.Find(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2024-02-20T01:12:31Z")));
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
    public async Task GetByTelegramIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTelegramIdAsync(12345);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TelegramId, Is.EqualTo(12345));
        Assert.That(result.CreatedAt, Is.EqualTo(DateTimeOffset.Parse("2024-02-20T01:12:31Z")));
    }

    [Test]
    public async Task GetByTelegramIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByTelegramIdAsync(99999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") },
            new User { Id = 2, TelegramId = 67890, CreatedAt = DateTimeOffset.Parse("2024-03-20T01:12:31Z") },
            new User { Id = 3, TelegramId = 11111, CreatedAt = DateTimeOffset.Parse("2024-04-20T01:12:31Z") }
        };
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task GetByIdsAsync_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") },
            new User { Id = 2, TelegramId = 67890, CreatedAt = DateTimeOffset.Parse("2024-03-20T01:12:31Z") },
            new User { Id = 3, TelegramId = 11111, CreatedAt = DateTimeOffset.Parse("2024-04-20T01:12:31Z") }
        };
        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var idsToFind = new List<int> { 1, 3 };

        // Act
        var result = await _repository.GetByIdsAsync(idsToFind);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(u => u.Id == 1), Is.True);
        Assert.That(result.Any(u => u.Id == 3), Is.True);
        Assert.That(result.Any(u => u.Id == 2), Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var user = new User { Id = 1, TelegramId = 12345, CreatedAt = DateTimeOffset.Parse("2024-02-20T01:12:31Z") };
        _repository.Add(user);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Users.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }
}
