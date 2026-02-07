using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DataAccessTests.Repositories;

[TestFixture]
public class SelectedLessonSourceRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<SelectedLessonSourceRepository>> _loggerMock;
    private SelectedLessonSourceRepository _repository;
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

        _loggerMock = new Mock<ILogger<SelectedLessonSourceRepository>>();
        _repository = new SelectedLessonSourceRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddSelectedSourceToContext()
    {
        // Arrange
        var selected = new SelectedLessonSource 
        { 
            Id = 1, 
            UserId = 1,
            SourceId = 1,
            LessonSourceType = LessonSourceType.Group,
            SourceName = "Source 1",
        };

        // Act
        _repository.Add(selected);
        _context.SaveChanges();

        // Assert
        var result = _context.SelectedLessonSources.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByUserId_ShouldReturnUserSources()
    {
        // Arrange
        var sources = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { Id = 1, UserId = 1, SourceId = 1, SourceName = "Source 1", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 2, UserId = 1, SourceId = 2, SourceName = "Source 2", LessonSourceType = LessonSourceType.Elective },
            new SelectedLessonSource { Id = 3, UserId = 2, SourceId = 1, SourceName = "Source 3", LessonSourceType = LessonSourceType.Group }
        };
        _context.SelectedLessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserId(1);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(s => s.UserId == 1), Is.True);
    }

    [Test]
    public async Task GetByUserIds_ShouldReturnMatchingUserSources()
    {
        // Arrange
        var sources = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { Id = 1, UserId = 1, SourceId = 1, SourceName = "Source 1", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 2, UserId = 2, SourceId = 2, SourceName = "Source 2", LessonSourceType = LessonSourceType.Elective },
            new SelectedLessonSource { Id = 3, UserId = 3, SourceId = 1, SourceName = "Source 3", LessonSourceType = LessonSourceType.Group }
        };
        _context.SelectedLessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        var userIds = new List<int> { 1, 3 };

        // Act
        var result = await _repository.GetByUserIds(userIds);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(s => s.UserId == 1), Is.True);
        Assert.That(result.Any(s => s.UserId == 3), Is.True);
    }

    [Test]
    public async Task GetBySourceIds_ShouldReturnMatchingSources()
    {
        // Arrange
        var sources = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { Id = 1, UserId = 1, SourceId = 1, SourceName = "Source 1", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 2, UserId = 2, SourceId = 2, SourceName = "Source 2", LessonSourceType = LessonSourceType.Elective },
            new SelectedLessonSource { Id = 3, UserId = 3, SourceId = 1, SourceName = "Source 3", LessonSourceType = LessonSourceType.Group }
        };
        _context.SelectedLessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int> { 1 };

        // Act
        var result = await _repository.GetBySourceIds(sourceIds);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(s => s.SourceId == 1), Is.True);
    }

    [Test]
    public async Task GetByUserIdAndSourceType_ShouldReturnMatchingSources()
    {
        // Arrange
        var sources = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { Id = 1, UserId = 1, SourceId = 1, SourceName = "Source 1", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 2, UserId = 1, SourceId = 2, SourceName = "Source 2", LessonSourceType = LessonSourceType.Elective },
            new SelectedLessonSource { Id = 3, UserId = 2, SourceId = 1, SourceName = "Source 3", LessonSourceType = LessonSourceType.Group }
        };
        _context.SelectedLessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAndSourceType(1, LessonSourceType.Group);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First().UserId, Is.EqualTo(1));
        Assert.That(result.First().LessonSourceType, Is.EqualTo(LessonSourceType.Group));
    }

    [Test]
    public async Task GetByUserIdsAndSourceType_ShouldReturnMatchingSources()
    {
        // Arrange
        var sources = new List<SelectedLessonSource>
        {
            new SelectedLessonSource { Id = 1, UserId = 1, SourceId = 1, SourceName = "Source 1", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 2, UserId = 1, SourceId = 2, SourceName = "Source 2", LessonSourceType = LessonSourceType.Elective },
            new SelectedLessonSource { Id = 3, UserId = 2, SourceId = 1, SourceName = "Source 3", LessonSourceType = LessonSourceType.Group },
            new SelectedLessonSource { Id = 4, UserId = 3, SourceId = 3, SourceName = "Source 4", LessonSourceType = LessonSourceType.Elective }
        };
        _context.SelectedLessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        var userIds = new List<int> { 1, 2 };

        // Act
        var result = await _repository.GetByUserIdsAndSourceType(userIds, LessonSourceType.Group);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(s => s.LessonSourceType == LessonSourceType.Group), Is.True);
        Assert.That(result.Any(s => s.UserId == 1), Is.True);
        Assert.That(result.Any(s => s.UserId == 2), Is.True);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var selected = new SelectedLessonSource 
        { 
            Id = 1, 
            UserId = 1,
            SourceId = 1,
            LessonSourceType = LessonSourceType.Group,
            SourceName = "Source 1",
        };
        _repository.Add(selected);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.SelectedLessonSources.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }
}
