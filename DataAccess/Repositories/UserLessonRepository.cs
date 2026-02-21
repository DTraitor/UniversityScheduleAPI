using Common.Enums;
using Common.Models.Internal;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserLessonRepository : IUserLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserLessonRepository> _logger;

    public UserLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<UserLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(UserLesson entity)
    {
        _context.UserLessons.Add(entity);
    }

    public void Update(UserLesson entity)
    {
        _context.UserLessons.Update(entity);
    }

    public void Delete(UserLesson entity)
    {
        _context.UserLessons.Remove(entity);
    }

    public async Task AddRangeAsync(ICollection<UserLesson> entities)
    {
        await _context.BulkInsertAsync(entities);
    }

    public async Task UpdateRangeAsync(ICollection<UserLesson> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<UserLesson> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<UserLesson?> GetByIdAsync(int id)
    {
        return await _context.UserLessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<UserLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserLessons.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<UserLesson>> GetByIdsAsync(ICollection<int> ids)
    {
        return await _context.UserLessons.Where(ul => ids.Contains(ul.Id)).ToListAsync();
    }

    public async Task<ICollection<int>> RemoveByUserIdsAsync(ICollection<int> userIds)
    {
        var lessons = _context.UserLessons.Where(ul => userIds.Contains(ul.UserId)).ToArray();
        await _context.BulkDeleteAsync(lessons);
        return lessons.Select(x => x.Id).ToList();
    }

    public async Task<ICollection<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIdsAsync(ICollection<int> userIds, SelectedLessonSourceType sourceType,
        ICollection<int> sourceIds)
    {
        var lessons = await _context.UserLessons.Where(ul =>
            userIds.Contains(ul.UserId) &&
            (ul.SelectedLessonSourceType & sourceType) == sourceType &&
            sourceIds.Contains(ul.LessonSourceId)).ToListAsync();
        var lessonIds = lessons.Select(x => x.Id).ToList();
        await _context.BulkDeleteAsync(lessons);
        return lessonIds;
    }

    public async Task<ICollection<UserLesson>> GetWithOccurrencesCalculatedDateLessThanAsync(DateTimeOffset dateTime)
    {
        return await _context.UserLessons
            .Where(l => l.OccurrencesCalculatedTill == null || l.OccurrencesCalculatedTill < dateTime)
            .Where(x => x.OccurrencesCalculatedTill == null || x.OccurrencesCalculatedTill < x.EndTime)
            .Where(x => x.RepeatType != RepeatType.Never || x.OccurrencesCalculatedTill == null)
            .ToListAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}