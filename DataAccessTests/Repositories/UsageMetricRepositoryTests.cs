using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class UsageMetricRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UsageMetricRepository>> _loggerMock;
    private UsageMetricRepository _repository;
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

        _loggerMock = new Mock<ILogger<UsageMetricRepository>>();
        _repository = new UsageMetricRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddMetricToContext()
    {
        // Arrange
        var metric = new UsageMetric { Id = 1, UserId = 1, Timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-02-01T00:00:00Z") };

        // Act
        _repository.Add(metric);
        _context.SaveChanges();

        // Assert
        var result = _context.UsageMetrics.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Timestamp, Is.EqualTo(DateTimeOffset.Parse("2024-01-01T00:00:00Z")));
        Assert.That(result.ScheduleTime, Is.EqualTo(DateTimeOffset.Parse("2024-02-01T00:00:00Z")));
    }

    [Test]
    public void Update_ShouldUpdateMetricInContext()
    {
        // Arrange
        var metric = new UsageMetric { Id = 1, UserId = 1, Timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-02-01T00:00:00Z") };
        _context.UsageMetrics.Add(metric);
        _context.SaveChanges();
        _context.Entry(metric).State = EntityState.Detached;

        var updated = new UsageMetric { Id = 1, UserId = 1, Timestamp = DateTimeOffset.Parse("2025-01-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2025-02-01T00:00:00Z") };

        // Act
        _repository.Update(updated);
        _context.SaveChanges();

        // Assert
        var result = _context.UsageMetrics.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Timestamp, Is.EqualTo(DateTimeOffset.Parse("2025-01-01T00:00:00Z")));
        Assert.That(result.ScheduleTime, Is.EqualTo(DateTimeOffset.Parse("2025-02-01T00:00:00Z")));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnMetric_WhenExists()
    {
        // Arrange
        var metric = new UsageMetric { Id = 1, UserId = 1, Timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-02-01T00:00:00Z") };
        _context.UsageMetrics.Add(metric);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllMetrics()
    {
        // Arrange
        var metrics = new List<UsageMetric>
        {
            new UsageMetric { Id = 1, UserId = 1, Timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-02-01T00:00:00Z") },
            new UsageMetric { Id = 2, UserId = 2, Timestamp = DateTimeOffset.Parse("2024-03-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-04-01T00:00:00Z") },
            new UsageMetric { Id = 3, UserId = 1, Timestamp = DateTimeOffset.Parse("2024-05-01T00:00:00Z"), ScheduleTime = DateTimeOffset.Parse("2024-06-01T00:00:00Z") }
        };
        _context.UsageMetrics.AddRange(metrics);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }
}
