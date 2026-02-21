using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Result;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly IUserModifiedRepository _userModifiedRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILessonSourceRepository _lessonSourceRepository;
    private readonly ISelectedLessonSourceRepository _selectedLessonSourceRepository;
    private readonly ISelectedElectiveLessonRepository selectedElectiveLessonRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserModifiedRepository userModifiedRepository,
        IUserRepository userRepository,
        ILessonSourceRepository lessonSourceRepository,
        ISelectedLessonSourceRepository selectedLessonSourceRepository,
        ISelectedElectiveLessonRepository selectedElectiveLessonRepository,
        ILogger<UserService> logger)
    {
        _userModifiedRepository = userModifiedRepository;
        _userRepository = userRepository;
        _lessonSourceRepository = lessonSourceRepository;
        _selectedLessonSourceRepository = selectedLessonSourceRepository;
        this.selectedElectiveLessonRepository = selectedElectiveLessonRepository;
        _logger = logger;
    }

    public async Task CreateUserAsync(long telegramId)
    {
        await using var transaction = await _userRepository.BeginTransactionAsync();

        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user is not null)
            return;

        _userRepository.Add(new User{ CreatedAt = DateTimeOffset.UtcNow, TelegramId = telegramId });

        await _userRepository.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task<Result> ChangeGroupAsync(long telegramId, string groupName, int subgroupNumber)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user is null)
            return ErrorType.UserNotFound;

        var groups = await _lessonSourceRepository.GetByNameAndLimitAsync(groupName.ToLower(), 2);
        if (groups.Count != 1 || groups.First().SourceType != LessonSourceType.Group)
            return ErrorType.GroupNotFound;

        var selectedGroup = await _selectedLessonSourceRepository.GetByUserId(user.Id);

        await using var transaction = await _selectedLessonSourceRepository.BeginTransactionAsync();

        await _selectedLessonSourceRepository.RemoveRangeAsync(selectedGroup);

        var newGroup = groups.First();

        _selectedLessonSourceRepository.Add(new SelectedLessonSource
        {
            LessonSourceType = LessonSourceType.Group,
            SourceId = newGroup.Id,
            SourceName = newGroup.Name,
            SubGroupNumber = subgroupNumber,
            UserId = user.Id,
        });

        _userModifiedRepository.Add(user.Id);

        await _selectedLessonSourceRepository.SaveChangesAsync();

        await transaction.CommitAsync();

        return Result.Success();
    }

    public async Task<Result<ICollection<SelectedElectiveLesson>>> GetUserElectiveLessonAsync(long telegramId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user is null)
            return ErrorType.UserNotFound;

        return new(await selectedElectiveLessonRepository.GetByUserId(user.Id));
    }

    public async Task<Result> AddUserElectiveLessonAsync(long telegramId, int sourceId, string lessonName,
        string? lessonType, int subgroupNumber)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user is null)
            return ErrorType.UserNotFound;

        var group = await _lessonSourceRepository.GetByIdAsync(sourceId);
        if (group is null || group.SourceType != LessonSourceType.Elective)
            return ErrorType.GroupNotFound;

        await using var transaction = await selectedElectiveLessonRepository.BeginTransactionAsync();

        selectedElectiveLessonRepository.Add(new SelectedElectiveLesson
        {
            LessonSourceId = sourceId,
            LessonName = lessonName,
            LessonType = lessonType,
            SubgroupNumber = subgroupNumber,
            UserId = user.Id,
        });

        _userModifiedRepository.Add(user.Id);

        await selectedElectiveLessonRepository.SaveChangesAsync();

        await transaction.CommitAsync();

        return Result.Success();
    }

    public async Task<Result> RemoveUserElectiveLessonAsync(long telegramId, int electiveLessonId)
    {
        var user = await _userRepository.GetByTelegramIdAsync(telegramId);
        if (user is null)
            return ErrorType.UserNotFound;

        var lesson = await selectedElectiveLessonRepository.GetByIdAsync(electiveLessonId);
        if (lesson is null || lesson.UserId != user.Id)
            return ErrorType.NotFound;

        await using var transaction = await selectedElectiveLessonRepository.BeginTransactionAsync();

        selectedElectiveLessonRepository.Delete(lesson);

        _userModifiedRepository.Add(user.Id);

        await selectedElectiveLessonRepository.SaveChangesAsync();

        await transaction.CommitAsync();

        return Result.Success();
    }
}