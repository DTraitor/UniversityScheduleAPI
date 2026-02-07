using Common.Enums;
using Common.Models.Internal;
using DataAccess.Domain;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class UserLessonRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<UserLessonRepository>> _loggerMock;
    private UserLessonRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ScheduleDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<UserLessonRepository>>();
        _repository = new UserLessonRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public void Add_ShouldAddLessonToContext()
    {
        // Arrange
        var lesson = new UserLesson 
        { 
            Id = 1, 
            UserId = 1, 
            LessonSourceId = 1,
            Title = "Title 1",
            Teacher = [ "Teacher 1", "Teacher 2" ],
            RepeatType = RepeatType.Daily
        };

        // Act
        _repository.Add(lesson);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessons.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RepeatType, Is.EqualTo(RepeatType.Daily));
    }

    [Test]
    public void Update_ShouldUpdateLessonInContext()
    {
        // Arrange
        var lesson = new UserLesson 
        { 
            Id = 1, 
            UserId = 1, 
            LessonSourceId = 1,
            Title = "Title 1",
            RepeatType = RepeatType.Daily,
            Teacher = [ "Teacher 1" ],
        };
        _context.UserLessons.Add(lesson);
        _context.SaveChanges();
        _context.Entry(lesson).State = EntityState.Detached;

        var updatedLesson = new UserLesson 
        { 
            Id = 1, 
            UserId = 1, 
            LessonSourceId = 1,
            RepeatType = RepeatType.Weekly,
            Title = "Title 2",
            Teacher = [ "Teacher 2" ],
        };

        // Act
        _repository.Update(updatedLesson);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessons.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.RepeatType, Is.EqualTo(RepeatType.Weekly));
    }

    [Test]
    public void Delete_ShouldRemoveLessonFromContext()
    {
        // Arrange
        var lesson = new UserLesson
        {
            Id = 1,
            UserId = 1,
            LessonSourceId = 1,
            Title = "Title 1",
            Teacher = [ "Teacher 1", "Teacher 2" ],
        };
        _context.UserLessons.Add(lesson);
        _context.SaveChanges();

        // Act
        _repository.Delete(lesson);
        _context.SaveChanges();

        // Assert
        var result = _context.UserLessons.Find(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnLesson_WhenExists()
    {
        // Arrange
        var lesson = new UserLesson
        {
            Id = 1,
            UserId = 1,
            LessonSourceId = 1,
            Title = "Title 1",
            Teacher = [ "Teacher 1", "Teacher 2" ],
        };
        _context.UserLessons.Add(lesson);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdsAsync_ShouldReturnMatchingLessons()
    {
        // Arrange
        var lessons = new List<UserLesson>
        {
            new UserLesson { Id = 1, UserId = 1, LessonSourceId = 1, Title = "Title 1", Teacher = [ "Teacher 1", "Teacher 2" ] },
            new UserLesson { Id = 2, UserId = 1, LessonSourceId = 2, Title = "Title 1", Teacher = [ "Teacher 3", "Teacher 4" ] },
            new UserLesson { Id = 3, UserId = 2, LessonSourceId = 1, Title = "Title 1", Teacher = [ "Teacher 5", "Teacher 6" ] }
        };
        _context.UserLessons.AddRange(lessons);
        await _context.SaveChangesAsync();

        var idsToFind = new List<int> { 1, 3 };

        // Act
        var result = await _repository.GetByIdsAsync(idsToFind);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(l => l.Id == 1), Is.True);
        Assert.That(result.Any(l => l.Id == 3), Is.True);
    }

    [Test]
    public async Task GetWithOccurrencesCalculatedDateLessThanAsync_ShouldReturnMatchingLessons()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var past = now.AddDays(-5);
        var future = now.AddDays(5);

        var lessons = new List<UserLesson>
        {
            new UserLesson 
            { 
                Id = 1, 
                UserId = 1, 
                LessonSourceId = 1,
                OccurrencesCalculatedTill = past,
                EndTime = future,
                RepeatType = RepeatType.Daily,
                Title = "Title 1",
                Teacher = [ "Teacher 1", "Teacher 2" ],
            },
            new UserLesson 
            { 
                Id = 2, 
                UserId = 1, 
                LessonSourceId = 2,
                OccurrencesCalculatedTill = future,
                EndTime = future.AddDays(10),
                RepeatType = RepeatType.Weekly,
                Title = "Title 1",
                Teacher = [ "Teacher 3", "Teacher 4" ],
            },
            new UserLesson 
            { 
                Id = 3, 
                UserId = 2, 
                LessonSourceId = 1,
                OccurrencesCalculatedTill = null,
                EndTime = future,
                RepeatType = RepeatType.Monthly,
                Title = "Title 1",
                Teacher = [ "Teacher 5", "Teacher 6" ],
            }
        };
        _context.UserLessons.AddRange(lessons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWithOccurrencesCalculatedDateLessThanAsync(now);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(l => l.Id == 1), Is.True);
        Assert.That(result.Any(l => l.Id == 3), Is.True);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var lesson = new UserLesson { Id = 1, UserId = 1, LessonSourceId = 1, Title = "Title 1", Teacher = [ "Teacher 1", "Teacher 2" ] };
        _repository.Add(lesson);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.UserLessons.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }
}
