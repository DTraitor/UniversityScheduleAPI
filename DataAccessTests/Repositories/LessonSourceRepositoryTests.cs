using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class LessonSourceRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<LessonSourceRepository>> _loggerMock;
    private LessonSourceRepository _repository;
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

        _loggerMock = new Mock<ILogger<LessonSourceRepository>>();
        _repository = new LessonSourceRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddLessonSourceToContext()
    {
        // Arrange
        var source = new LessonSource 
        { 
            Id = 1, 
            Name = "Mathematics",
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
            SourceType = LessonSourceType.Group,
        };

        // Act
        _repository.Add(source);
        _context.SaveChanges();

        // Assert
        var result = _context.LessonSources.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Mathematics"));
    }

    [Test]
    public void Update_ShouldUpdateLessonSourceInContext()
    {
        // Arrange
        var source = new LessonSource 
        { 
            Id = 1, 
            Name = "Original",
            SourceType = LessonSourceType.Group,
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
        };
        _context.LessonSources.Add(source);
        _context.SaveChanges();
        _context.Entry(source).State = EntityState.Detached;

        var updatedSource = new LessonSource 
        { 
            Id = 1, 
            Name = "Updated",
            SourceType = LessonSourceType.Group,
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
        };

        // Act
        _repository.Update(updatedSource);
        _context.SaveChanges();

        // Assert
        var result = _context.LessonSources.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Updated"));
    }

    [Test]
    public void Delete_ShouldRemoveLessonSourceFromContext()
    {
        // Arrange
        var source = new LessonSource 
        { 
            Id = 1, 
            Name = "ToDelete",
            SourceType = LessonSourceType.Group,
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
        };
        _context.LessonSources.Add(source);
        _context.SaveChanges();

        // Act
        _repository.Delete(source);
        _context.SaveChanges();

        // Assert
        var result = _context.LessonSources.Find(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnLessonSource_WhenExists()
    {
        // Arrange
        var source = new LessonSource 
        { 
            Id = 1, 
            Name = "Physics",
            SourceType = LessonSourceType.Group,
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
        };
        _context.LessonSources.Add(source);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Physics"));
    }

    [Test]
    public async Task GetByNameAndSourceTypeAsync_ShouldReturnMatchingSource()
    {
        // Arrange
        var sources = new List<LessonSource>
        {
            new LessonSource { Id = 1, Name = "Math", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 2, Name = "Math", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Elective },
            new LessonSource { Id = 3, Name = "Physics", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group }
        };
        _context.LessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAndSourceTypeAsync("Math", LessonSourceType.Group);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.Name, Is.EqualTo("Math"));
        Assert.That(result.SourceType, Is.EqualTo(LessonSourceType.Group));
    }

    [Test]
    public async Task GetByNameAndLimitAsync_ShouldReturnMatchingSourcesWithLimit()
    {
        // Arrange
        var sources = new List<LessonSource>
        {
            new LessonSource { Id = 1, Name = "Mathematics", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 2, Name = "Math 101", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Elective },
            new LessonSource { Id = 3, Name = "Advanced Math", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 4, Name = "Physics", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group }
        };
        _context.LessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAndLimitAsync("math", 2);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.All(s => s.Name.ToLower().Contains("math")), Is.True);
    }

    [Test]
    public async Task GetByIdsAsync_ShouldReturnMatchingSources()
    {
        // Arrange
        var sources = new List<LessonSource>
        {
            new LessonSource { Id = 1, Name = "Math", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 2, Name = "Physics", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 3, Name = "Chemistry", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group }
        };
        _context.LessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        var idsToFind = new List<int> { 1, 3 };

        // Act
        var result = await _repository.GetByIdsAsync(idsToFind);

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(s => s.Id == 1), Is.True);
        Assert.That(result.Any(s => s.Id == 3), Is.True);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllSources()
    {
        // Arrange
        var sources = new List<LessonSource>
        {
            new LessonSource { Id = 1, Name = "Math", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 2, Name = "Physics", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group },
            new LessonSource { Id = 3, Name = "Chemistry", HrefId = "some id 1", TimeZone = "Europe/Kyiv", PageHash = "123123", SourceType = LessonSourceType.Group }
        };
        _context.LessonSources.AddRange(sources);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var source = new LessonSource 
        { 
            Id = 1, 
            Name = "Test",
            SourceType = LessonSourceType.Group,
            HrefId = "some id 1",
            TimeZone = "Europe/Kyiv",
            PageHash = "123123",
        };
        _repository.Add(source);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.LessonSources.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }
}
