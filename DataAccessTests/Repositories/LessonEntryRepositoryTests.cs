using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DataAccessTests.Repositories;

[TestFixture]
public class LessonEntryRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<LessonEntryRepository>> _mockLogger;
    private LessonEntryRepository _repository;
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

        _mockLogger = new Mock<ILogger<LessonEntryRepository>>();
        _repository = new LessonEntryRepository(_context, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    #region Add Tests

    [Test]
    public void Add_ShouldAddEntityToContext()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };

        // Act
        _repository.Add(lessonEntry);

        // Assert
        var entry = _context.Entry(lessonEntry);
        Assert.That(entry.State, Is.EqualTo(EntityState.Added));
    }

    [Test]
    public async Task Add_ShouldPersistEntityAfterSaveChanges()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };

        // Act
        _repository.Add(lessonEntry);
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.LessonEntries.FindAsync(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SourceId, Is.EqualTo(100));
    }

    #endregion

    #region Update Tests

    [Test]
    public async Task Update_ShouldUpdateExistingEntity()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        // Detach to simulate getting entity from another context
        _context.Entry(lessonEntry).State = EntityState.Detached;

        // Modify the entity
        lessonEntry.SourceId = 200;

        // Act
        _repository.Update(lessonEntry);
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.LessonEntries.FindAsync(1);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SourceId, Is.EqualTo(200));
    }

    [Test]
    public void Update_ShouldMarkEntityAsModified()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };

        // Act
        _repository.Update(lessonEntry);

        // Assert
        var entry = _context.Entry(lessonEntry);
        Assert.That(entry.State, Is.EqualTo(EntityState.Modified));
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(lessonEntry);
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.LessonEntries.FindAsync(1);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Delete_ShouldMarkEntityAsDeleted()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        _context.SaveChanges();

        // Act
        _repository.Delete(lessonEntry);

        // Assert
        var entry = _context.Entry(lessonEntry);
        Assert.That(entry.State, Is.EqualTo(EntityState.Deleted));
    }

    #endregion

    #region AddRangeAsync Tests

    [Test]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 101, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 102, Title = "Test Lesson 3" }
        };

        // Act
        await _repository.AddRangeAsync(lessonEntries);

        // Assert
        var result = await _context.LessonEntries.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result.Any(x => x.Id == 1 && x.SourceId == 100), Is.True);
        Assert.That(result.Any(x => x.Id == 2 && x.SourceId == 101), Is.True);
        Assert.That(result.Any(x => x.Id == 3 && x.SourceId == 102), Is.True);
    }

    [Test]
    public async Task AddRangeAsync_ShouldHandleEmptyCollection()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>();

        // Act
        await _repository.AddRangeAsync(lessonEntries);

        // Assert
        var result = await _context.LessonEntries.ToListAsync();
        Assert.That(result, Is.Empty);
    }

    #endregion

    #region UpdateRangeAsync Tests

    [Test]
    public async Task UpdateRangeAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 101, Title = "Test Lesson 2" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        // Modify entities
        lessonEntries[0].SourceId = 200;
        lessonEntries[1].SourceId = 201;

        // Act
        await _repository.UpdateRangeAsync(lessonEntries);

        // Assert
        var result = await _context.LessonEntries.ToListAsync();
        Assert.That(result.First(x => x.Id == 1).SourceId, Is.EqualTo(200));
        Assert.That(result.First(x => x.Id == 2).SourceId, Is.EqualTo(201));
    }

    #endregion

    #region RemoveRangeAsync Tests

    [Test]
    public async Task RemoveRangeAsync_ShouldRemoveMultipleEntities()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 101, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 102, Title = "Test Lesson 3" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        var entriesToRemove = lessonEntries.Take(2).ToList();

        // Act
        await _repository.RemoveRangeAsync(entriesToRemove);

        // Assert
        var result = await _context.LessonEntries.ToListAsync();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(3));
    }

    #endregion

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ShouldReturnEntityWhenExists()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
        Assert.That(result.SourceId, Is.EqualTo(100));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNullWhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 101, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 102, Title = "Test Lesson 3" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnEmptyListWhenNoEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetAllAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _repository.GetAllAsync(cts.Token));
    }

    #endregion

    #region SaveChangesAsync Tests

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.LessonEntries.FindAsync(1);
        Assert.That(result, Is.Not.Null);
    }

    #endregion

    #region GetBySourceIdAsync Tests

    [Test]
    public async Task GetBySourceIdAsync_ShouldReturnEntitiesWithMatchingSourceId()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 100, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 101, Title = "Test Lesson 3" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySourceIdAsync(100);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.All(entry => entry.SourceId == 100), Is.True);
    }

    [Test]
    public async Task GetBySourceIdAsync_ShouldReturnEmptyListWhenNoMatch()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySourceIdAsync(999);

        // Assert
        Assert.That(result, Is.Empty);
    }

    #endregion

    #region GetBySourceIdsAsync Tests

    [Test]
    public async Task GetBySourceIdsAsync_ShouldReturnEntitiesWithMatchingSourceIds()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 101, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 102, Title = "Test Lesson 3" },
            new LessonEntry { Id = 4, SourceId = 103, Title = "Test Lesson 4" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int> { 100, 102 };

        // Act
        var result = await _repository.GetBySourceIdsAsync(sourceIds);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Any(x => x.SourceId == 100), Is.True);
        Assert.That(result.Any(x => x.SourceId == 102), Is.True);
        Assert.That(result.Any(x => x.SourceId == 101), Is.False);
        Assert.That(result.Any(x => x.SourceId == 103), Is.False);
    }

    [Test]
    public async Task GetBySourceIdsAsync_ShouldReturnEmptyListWhenNoMatch()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int> { 200, 300 };

        // Act
        var result = await _repository.GetBySourceIdsAsync(sourceIds);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetBySourceIdsAsync_ShouldHandleEmptySourceIdsList()
    {
        // Arrange
        var lessonEntry = new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson" };
        _context.LessonEntries.Add(lessonEntry);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int>();

        // Act
        var result = await _repository.GetBySourceIdsAsync(sourceIds);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetBySourceIdsAsync_ShouldReturnAllMatchingEntries()
    {
        // Arrange
        var lessonEntries = new List<LessonEntry>
        {
            new LessonEntry { Id = 1, SourceId = 100, Title = "Test Lesson 1" },
            new LessonEntry { Id = 2, SourceId = 100, Title = "Test Lesson 2" },
            new LessonEntry { Id = 3, SourceId = 101, Title = "Test Lesson 3" },
            new LessonEntry { Id = 4, SourceId = 101, Title = "Test Lesson 4" }
        };
        await _context.LessonEntries.AddRangeAsync(lessonEntries);
        await _context.SaveChangesAsync();

        var sourceIds = new List<int> { 100, 101 };

        // Act
        var result = await _repository.GetBySourceIdsAsync(sourceIds);

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
    }

    #endregion
}