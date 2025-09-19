using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.ElectiveLessons;

public class ElectiveChangeHandler : IChangeHandler<ElectiveLesson>
{
    private readonly IElectiveLessonDayRepository _dayRepository;
    private readonly IElectiveLessonRepository _lessonRepository;
    private readonly IElectedLessonRepository _electedRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserLessonRepository _userLessonRepository;
    private readonly IUserLessonOccurenceRepository _userLessonOccurenceRepository;
    private readonly ILogger<ElectiveChangeHandler> _logger;

    public ElectiveChangeHandler(
        IElectiveLessonDayRepository dayRepository,
        IElectiveLessonRepository lessonRepository,
        IElectedLessonRepository electedRepository,
        IUserRepository userRepository,
        IUserLessonRepository userLessonRepository,
        IUserLessonOccurenceRepository userLessonOccurenceRepository,
        ILogger<ElectiveChangeHandler> logger)
    {
        _dayRepository = dayRepository;
        _lessonRepository = lessonRepository;
        _electedRepository = electedRepository;
        _userRepository = userRepository;
        _userLessonRepository = userLessonRepository;
        _userLessonOccurenceRepository = userLessonOccurenceRepository;
        _logger = logger;
    }

    public async Task HandleChanges(IEnumerable<ElectiveLesson> oldLessons, IEnumerable<ElectiveLesson> newLessons, CancellationToken token)
    {
        return;
        //handle new lessons
        throw new NotImplementedException();
    }
}