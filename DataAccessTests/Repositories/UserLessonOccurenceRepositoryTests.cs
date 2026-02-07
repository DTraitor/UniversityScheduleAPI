using DataAccess.Domain;
using DataAccess.Models.Internal;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DataAccessTests.Repositories;

[TestFixture]
public class UserLessonOccurenceRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UserLessonOccurenceRepository>> _loggerMock;
    private UserLessonOccurenceRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ScheduleDbContext(options);
        _loggerMock = new Mock<ILogger<UserLessonOccurenceRepository>>();
        _repository = new UserLessonOccurenceRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public void Add_ShouldAddOccurrenceToContext()
    {
        // Arrange
        var occurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 1,
            StartTime = DateTimeOffset.UtcNow 
        };

        // Act
        _repository.Add(occurrence);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessonOccurrences.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LessonId, Is.EqualTo(1));
    }

    [Test]
    public void Update_ShouldUpdateOccurrenceInContext()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow;
        var occurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 1,
            StartTime = startTime 
        };
        _context.UserLessonOccurrences.Add(occurrence);
        _context.SaveChanges();
        _context.Entry(occurrence).State = EntityState.Detached;

        var updatedOccurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 2,
            StartTime = startTime 
        };

        // Act
        _repository.Update(updatedOccurrence);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessonOccurrences.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.LessonId, Is.EqualTo(2));
    }

    [Test]
    public void Delete_ShouldRemoveOccurrenceFromContext()
    {
        // Arrange
        var occurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 1,
            StartTime = DateTimeOffset.UtcNow 
        };
        _context.UserLessonOccurrences.Add(occurrence);
        _context.SaveChanges();

        // Act
        _repository.Delete(occurrence);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessonOccurrences.Find(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnOccurrence_WhenExists()
    {
        // Arrange
        var occurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 1,
            StartTime = DateTimeOffset.UtcNow 
        };
        _context.UserLessonOccurrences.Add(occurrence);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByUserIdAndBetweenDateAsync_ShouldReturnMatchingOccurrences()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var occurrences = new List<UserLessonOccurrence>
        {
            new UserLessonOccurrence 
            { 
                Id = 1, 
                UserId = 1, 
                LessonId = 1,
                StartTime = now.AddDays(-2) 
            },
            new UserLessonOccurrence 
            { 
                Id = 2, 
                UserId = 1, 
                LessonId = 2,
                StartTime = now.AddDays(2) 
            },
            new UserLessonOccurrence 
            { 
                Id = 3, 
                UserId = 2, 
                LessonId = 1,
                StartTime = now.AddDays(1) 
            }
        };
        _context.UserLessonOccurrences.AddRange(occurrences);
        await _context.SaveChangesAsync();

        var beginDate = now.AddDays(-5);
        var endDate = now.AddDays(5);

        // Act
        var result = await _repository.GetByUserIdAndBetweenDateAsync(1, beginDate, endDate);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(o => o.UserId == 1), Is.True);
        Assert.That(result.Any(o => o.Id == 1), Is.True);
        Assert.That(result.Any(o => o.Id == 2), Is.True);
    }

    [Test]
    public async Task GetLatestOccurrenceAsync_ShouldReturnMostRecentOccurrence()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var occurrences = new List<UserLessonOccurrence>
        {
            new UserLessonOccurrence 
            { 
                Id = 1, 
                UserId = 1, 
                LessonId = 1,
                StartTime = now.AddDays(-5) 
            },
            new UserLessonOccurrence 
            { 
                Id = 2, 
                UserId = 1, 
                LessonId = 1,
                StartTime = now.AddDays(-1) // Most recent
            },
            new UserLessonOccurrence 
            { 
                Id = 3, 
                UserId = 2, 
                LessonId = 1,
                StartTime = now.AddDays(-3) 
            }
        };
        _context.UserLessonOccurrences.AddRange(occurrences);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestOccurrenceAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(2));
        Assert.That(result.StartTime.Date, Is.EqualTo(now.AddDays(-1).Date));
    }

    [Test]
    public async Task GetLatestOccurrenceAsync_ShouldReturnNull_WhenNoOccurrences()
    {
        // Act
        var result = await _repository.GetLatestOccurrenceAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var occurrence = new UserLessonOccurrence 
        { 
            Id = 1, 
            UserId = 1, 
            LessonId = 1,
            StartTime = DateTimeOffset.UtcNow 
        };
        _repository.Add(occurrence);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.UserLessonOccurrences.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }
}
