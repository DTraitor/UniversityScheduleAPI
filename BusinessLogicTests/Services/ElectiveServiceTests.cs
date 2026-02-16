using BusinessLogic.Services;
using Common.Enums;
using Common.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;

namespace BusinessLogicTests.Services;

[TestFixture]
public class ElectiveServiceTests
{
    private Mock<ILessonSourceRepository> _lessonSourceRepo;
    private Mock<ISelectedLessonSourceRepository> _selectedLessonSourceRepo;
    private Mock<ILessonEntryRepository> _lessonEntryRepo;
    private Mock<IUserModifiedRepository> _userModifiedRepo;
    private Mock<ISelectedElectiveLessonRepository> _selectedElectiveRepo;
    private Mock<IUserRepository> _userRepo;
    private Mock<ILogger<ElectiveService>> _logger;
    private ElectiveService _service;

    [SetUp]
    public void SetUp()
    {
        _lessonSourceRepo = new Mock<ILessonSourceRepository>();
        _selectedLessonSourceRepo = new Mock<ISelectedLessonSourceRepository>();
        _lessonEntryRepo = new Mock<ILessonEntryRepository>();
        _userModifiedRepo = new Mock<IUserModifiedRepository>();
        _selectedElectiveRepo = new Mock<ISelectedElectiveLessonRepository>();
        _userRepo = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<ElectiveService>>();

        _service = new ElectiveService(
            _lessonSourceRepo.Object,
            _selectedLessonSourceRepo.Object,
            _lessonEntryRepo.Object,
            _userModifiedRepo.Object,
            _selectedElectiveRepo.Object,
            _userRepo.Object,
            _logger.Object);
    }

    [Test]
    public async Task GetPossibleLevelsAsync_ReturnsElectiveSources()
    {
        var sources = new List<LessonSource> { new LessonSource { Id = 1 }, new LessonSource { Id = 2 } };
        _lessonSourceRepo.Setup(x => x.GetBySourceTypeAsync(LessonSourceType.Elective)).ReturnsAsync(sources);

        var res = await _service.GetPossibleLevelsAsync();

        Assert.That(res.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetLessonsByNameAsync_ReturnsDistinctTitles_UnderLimit()
    {
        var source = new LessonSource { Id = 5, SourceType = LessonSourceType.Elective };
        _lessonSourceRepo.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(source);

        var entries = new List<LessonEntry>
        {
            new LessonEntry { Title = "Programming 1" },
            new LessonEntry { Title = "Programming 2" },
            new LessonEntry { Title = "Programming 1" }
        };

        _lessonEntryRepo.Setup(x => x.GetBySourceIdAndPartialNameAsync(5, "prog")).ReturnsAsync(entries);

        var res = await _service.GetLessonsByNameAsync("prog", 5);

        Assert.That(res.IsSuccess, Is.True);
        var titles = res.Value.ToArray();
        CollectionAssert.AreEquivalent(new[] { "Programming 1", "Programming 2" }, titles);
    }

    [Test]
    public async Task GetLessonsByNameAsync_ReturnsTooManyElements_WhenMoreThan9()
    {
        var source = new LessonSource { Id = 6, SourceType = LessonSourceType.Elective };
        _lessonSourceRepo.Setup(x => x.GetByIdAsync(6)).ReturnsAsync(source);

        var entries = Enumerable.Range(0, 12).Select(i => new LessonEntry { Title = "L" + i }).ToList();
        _lessonEntryRepo.Setup(x => x.GetBySourceIdAndPartialNameAsync(6, "x")).ReturnsAsync(entries);

        var res = await _service.GetLessonsByNameAsync("x", 6);

        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(ErrorType.TooManyElements));
    }

    [Test]
    public async Task GetLessonTypesAsync_ReturnsNotFound_WhenEntriesMismatch()
    {
        var source = new LessonSource { Id = 7, SourceType = LessonSourceType.Elective };
        _lessonSourceRepo.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(source);

        var entries = new List<LessonEntry> { new LessonEntry { Title = "Other" } };
        _lessonEntryRepo.Setup(x => x.GetBySourceIdAndPartialNameAsync(7, "exactname")).ReturnsAsync(entries);

        var res = await _service.GetLessonTypesAsync("ExactName", 7);

        Assert.That(res.IsSuccess, Is.False);
        Assert.That(res.Error, Is.EqualTo(ErrorType.NotFound));
    }
}
