using DataAccess.Domain;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserLessonOccurenceRepository : IUserLessonOccurenceRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserLessonOccurenceRepository> _logger;

    public UserLessonOccurenceRepository(ScheduleDbContext scheduleDbContext, ILogger<UserLessonOccurenceRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(UserLessonOccurrence entity)
    {
        _context.UserLessonOccurrences.Add(entity);
    }

    public void Update(UserLessonOccurrence entity)
    {
        _context.UserLessonOccurrences.Update(entity);
    }

    public void Delete(UserLessonOccurrence entity)
    {
        _context.UserLessonOccurrences.Remove(entity);
    }

    public void AddRange(IEnumerable<UserLessonOccurrence> lessonOccurrences)
    {
        _context.FutureAction(x => x.BulkInsert(lessonOccurrences));
    }

    public void UpdateRange(IEnumerable<UserLessonOccurrence> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<UserLessonOccurrence> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<UserLessonOccurrence?> GetByIdAsync(int id)
    {
        return await _context.UserLessonOccurrences.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<UserLessonOccurrence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserLessonOccurrences.ToListAsync(cancellationToken);
    }

    public void ClearByLessonIds(IEnumerable<int> toRemove)
    {
        var entriesToRemove = _context.UserLessonOccurrences.Where(y => toRemove.Contains(y.LessonId)).AsEnumerable();
        _context.FutureAction(x => x.BulkDelete(entriesToRemove));
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
        _context.ExecuteFutureAction();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
        _context.ExecuteFutureAction();
    }

    public async Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndBetweenDateAsync(int userId, DateTimeOffset beginDate, DateTimeOffset endDate)
    {
        return await _context.UserLessonOccurrences
            .Where(x => x.UserId == userId)
            .Where(x => x.StartTime >= beginDate && x.StartTime < endDate)
            .ToListAsync();
    }

    public UserLessonOccurrence? GetLatestOccurrence(int lessonId)
    {
        return _context.UserLessonOccurrences
            .Where(x => x.LessonId == lessonId)
            .OrderByDescending(x => x.StartTime)
            .FirstOrDefault();
    }
}