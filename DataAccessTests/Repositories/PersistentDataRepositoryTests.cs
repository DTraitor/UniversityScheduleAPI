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
public class PersistentDataRepositoryTests
{
    private ScheduleDbContext _context;
    private Mock<ILogger<PersistentDataRepository>> _loggerMock;
    private PersistentDataRepository _repository;
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

        _loggerMock = new Mock<ILogger<PersistentDataRepository>>();
        _repository = new PersistentDataRepository(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Test]
    public void SetData_ShouldAddNewData_WhenKeyDoesNotExist()
    {
        // Arrange
        var data = new PersistentData { Key = "test-key", Value = "test-value" };

        // Act
        _repository.SetData(data);
        _context.SaveChanges();

        // Assert
        var result = _context.PersistentData.FirstOrDefault(x => x.Key == "test-key");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("test-value"));
    }

    [Test]
    public void SetData_ShouldUpdateExistingData_WhenKeyExists()
    {
        // Arrange
        var originalData = new PersistentData { Key = "test-key", Value = "original-value" };
        _context.PersistentData.Add(originalData);
        _context.SaveChanges();

        var updatedData = new PersistentData { Key = "test-key", Value = "updated-value" };

        // Act
        _repository.SetData(updatedData);
        _context.SaveChanges();

        // Assert
        var result = _context.PersistentData.FirstOrDefault(x => x.Key == "test-key");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("updated-value"));
        
        // Verify only one record exists
        var count = _context.PersistentData.Count(x => x.Key == "test-key");
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetData_ShouldReturnData_WhenKeyExists()
    {
        // Arrange
        var data = new PersistentData { Key = "test-key", Value = "test-value" };
        _context.PersistentData.Add(data);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetData("test-key");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Key, Is.EqualTo("test-key"));
        Assert.That(result.Value, Is.EqualTo("test-value"));
    }

    [Test]
    public async Task GetData_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Act
        var result = await _repository.GetData("non-existent-key");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var data = new PersistentData { Key = "test-key", Value = "test-value" };
        _repository.SetData(data);

        // Act
        await _repository.SaveChangesAsync(CancellationToken.None);

        // Assert
        var result = await _context.PersistentData.FirstOrDefaultAsync(x => x.Key == "test-key");
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void SetData_MultipleTimes_ShouldNotCreateDuplicates()
    {
        // Arrange
        var data1 = new PersistentData { Key = "test-key", Value = "value1" };
        var data2 = new PersistentData { Key = "test-key", Value = "value2" };
        var data3 = new PersistentData { Key = "test-key", Value = "value3" };

        // Act
        _repository.SetData(data1);
        _context.SaveChanges();
        _repository.SetData(data2);
        _context.SaveChanges();
        _repository.SetData(data3);
        _context.SaveChanges();

        // Assert
        var count = _context.PersistentData.Count(x => x.Key == "test-key");
        Assert.That(count, Is.EqualTo(1));
        
        var result = _context.PersistentData.FirstOrDefault(x => x.Key == "test-key");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo("value3"));
    }
}
