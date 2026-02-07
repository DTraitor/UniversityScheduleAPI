using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class SelectedElectiveLessonRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<SelectedElectiveLessonRepository>> _loggerMock;
    private SelectedElectiveLessonRepository _repository;
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

        _loggerMock = new Mock<ILogger<SelectedElectiveLessonRepository>>();
        _repository = new SelectedElectiveLessonRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddElectiveLessonToContext()
    {
        // Arrange
        var lesson = new SelectedElectiveLesson
        {
            Id = 1,
            UserId = 1,
            LessonSourceId = 1,
            LessonName = "Lesson 1",
        };

        // Act
        _repository.Add(lesson);
        _context.SaveChanges();

        // Assert
        var result = _context.SelectedElectiveLessonEntries.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(1));
    }

    [Test]
    public async Task GetBySourceIds_ShouldReturnMatchingLessons()
    {
        // Arrange
        var lessons = new List<SelectedElectiveLesson>
        {
            new SelectedElectiveLesson { Id = 1, UserId = 1, LessonSourceId = 1, LessonName = "Lesson 1" },
            new SelectedElectiveLesson { Id = 2, UserId = 2, LessonSourceId = 2, LessonName = "Lesson 2" },
            new SelectedElectiveLesson { Id = 3, UserId = 3, LessonSourceId = 1, LessonName = "Lesson 3" }
        };
        _context.SelectedElectiveLessonEntries.AddRange(lessons);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int> { 1 };

        // Act
        var result = await _repository.GetBySourceIds(sourceIds);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(l => l.LessonSourceId == 1), Is.True);
    }

    [Test]
    public async Task GetByUserIds_ShouldReturnMatchingLessons()
    {
        // Arrange
        var lessons = new List<SelectedElectiveLesson>
        {
            new SelectedElectiveLesson { Id = 1, UserId = 1, LessonSourceId = 1, LessonName = "Lesson 1" },
            new SelectedElectiveLesson { Id = 2, UserId = 2, LessonSourceId = 2, LessonName = "Lesson 2" },
            new SelectedElectiveLesson { Id = 3, UserId = 1, LessonSourceId = 3, LessonName = "Lesson 3" }
        };
        _context.SelectedElectiveLessonEntries.AddRange(lessons);
        await _context.SaveChangesAsync();

        var userIds = new List<int> { 1 };

        // Act
        var result = await _repository.GetByUserIds(userIds);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(l => l.UserId == 1), Is.True);
    }

    [Test]
    public async Task GetByUserId_ShouldReturnUserLessons()
    {
        // Arrange
        var lessons = new List<SelectedElectiveLesson>
        {
            new SelectedElectiveLesson { Id = 1, UserId = 1, LessonSourceId = 1, LessonName = "Lesson 1" },
            new SelectedElectiveLesson { Id = 2, UserId = 2, LessonSourceId = 2, LessonName = "Lesson 2" },
            new SelectedElectiveLesson { Id = 3, UserId = 1, LessonSourceId = 3, LessonName = "Lesson 3" }
        };
        _context.SelectedElectiveLessonEntries.AddRange(lessons);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserId(1);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(l => l.UserId == 1), Is.True);
    }
}
