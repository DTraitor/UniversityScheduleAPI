using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class LessonSourceModifiedRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<LessonSourceModifiedRepositoryRepository>> _loggerMock;
    private LessonSourceModifiedRepositoryRepository _repository;
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

        _loggerMock = new Mock<ILogger<LessonSourceModifiedRepositoryRepository>>();
        _repository = new LessonSourceModifiedRepositoryRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void Add_ShouldAddModificationToContext()
    {
        // Arrange
        var modification = new LessonSourceModified { Id = 1, SourceId = 1 };

        // Act
        _repository.Add(modification);
        _context.SaveChanges();

        // Assert
        var result = _context.LessonSourceModifications.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SourceId, Is.EqualTo(1));
    }

    [Test]
    public void Update_ShouldUpdateModificationInContext()
    {
        // Arrange
        var modification = new LessonSourceModified { Id = 1, SourceId = 1 };
        _context.LessonSourceModifications.Add(modification);
        _context.SaveChanges();
        _context.Entry(modification).State = EntityState.Detached;

        var updated = new LessonSourceModified { Id = 1, SourceId = 2 };

        // Act
        _repository.Update(updated);
        _context.SaveChanges();

        // Assert
        var result = _context.LessonSourceModifications.Find(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SourceId, Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnModification_WhenExists()
    {
        // Arrange
        var modification = new LessonSourceModified { Id = 1, SourceId = 1 };
        _context.LessonSourceModifications.Add(modification);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllModifications()
    {
        // Arrange
        var modifications = new List<LessonSourceModified>
        {
            new LessonSourceModified { Id = 1, SourceId = 1 },
            new LessonSourceModified { Id = 2, SourceId = 2 },
            new LessonSourceModified { Id = 3, SourceId = 3 }
        };
        _context.LessonSourceModifications.AddRange(modifications);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
    }
}
