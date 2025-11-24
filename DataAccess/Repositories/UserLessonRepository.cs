using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
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

    public void AddRange(IEnumerable<UserLesson> entities)
    {
        _context.FutureAction(x => x.BulkInsert(entities));
    }

    public void UpdateRange(IEnumerable<UserLesson> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<UserLesson> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<UserLesson?> GetByIdAsync(int id)
    {
        return await _context.UserLessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<UserLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserLessons.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        using var transaction = _context.Database.BeginTransaction();

        _context.ExecuteFutureAction();
        _context.SaveChanges();

        transaction.Commit();
    }

    public async Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.UserLessons.Where(ul => ids.Contains(ul.Id)).ToListAsync();
    }

    public IEnumerable<int> RemoveByUserIds(IEnumerable<int> userIds)
    {
        var lessons = _context.UserLessons.Where(ul => userIds.Contains(ul.UserId)).ToArray();
        _context.FutureAction(x => x.BulkDelete(lessons));
        return lessons.Select(x => x.Id);
    }

    public async Task<IEnumerable<int>> RemoveByUserIdsAndLessonSourceTypeAndLessonSourceIds(IEnumerable<int> userIds, SelectedLessonSourceType sourceType,
        IEnumerable<int> sourceIds)
    {
        var lessons = _context.UserLessons.Where(ul =>
            userIds.Contains(ul.UserId) &&
            (ul.SelectedLessonSourceType & sourceType) == sourceType &&
            sourceIds.Contains(ul.LessonSourceId));
        _context.FutureAction(x => x.BulkDelete(lessons));
        return await lessons.Select(x => x.Id).ToListAsync();
    }

    public async Task<IList<UserLesson>> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime)
    {
        return await _context.UserLessons
            .Where(l => l.OccurrencesCalculatedTill == null || l.OccurrencesCalculatedTill < dateTime)
            .Where(x => x.OccurrencesCalculatedTill < x.EndTime)
            .Where(x => x.RepeatType != RepeatType.Never || x.OccurrencesCalculatedTill == null)
            .ToListAsync();
    }
}