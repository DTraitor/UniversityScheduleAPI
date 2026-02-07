using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class UserModifiedRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UserModifiedRepository>> _loggerMock;
    private UserModifiedRepository _repository;
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

        _loggerMock = new Mock<ILogger<UserModifiedRepository>>();
        _repository = new UserModifiedRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddUserModifiedToContext()
    {
        // Arrange
        var userId = 1;

        // Act
        _repository.Add(userId);
        _context.SaveChanges();

        // Assert
        var result = _context.UserModifications.FirstOrDefault(x => x.UserId == userId);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
    }

    [Test]
    public async Task GetNotProcessedAsync_ShouldReturnUpTo100Records()
    {
        // Arrange
        var modifications = Enumerable.Range(1, 150)
            .Select(i => new UserModified { UserId = i })
            .ToList();

        _context.UserModifications.AddRange(modifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNotProcessedAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task GetNotProcessedAsync_ShouldReturnEmpty_WhenNoRecords()
    {
        // Act
        var result = await _repository.GetNotProcessedAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        _repository.Add(1);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var result = _context.UserModifications.FirstOrDefault(x => x.UserId == 1);
        Assert.That(result, Is.Not.Null);
    }
}
