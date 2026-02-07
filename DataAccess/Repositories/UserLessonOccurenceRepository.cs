using Common.Models.Internal;
using DataAccess.Domain;
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

    public async Task AddRangeAsync(ICollection<UserLessonOccurrence> lessonOccurrences)
    {
        await _context.BulkInsertAsync(lessonOccurrences);
    }

    public async Task UpdateRangeAsync(ICollection<UserLessonOccurrence> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<UserLessonOccurrence> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<UserLessonOccurrence?> GetByIdAsync(int id)
    {
        return await _context.UserLessonOccurrences.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<UserLessonOccurrence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserLessonOccurrences.ToListAsync(cancellationToken);
    }

    public async Task ClearByLessonIdsAsync(ICollection<int> toRemove)
    {
        var entriesToRemove = _context.UserLessonOccurrences.Where(y => toRemove.Contains(y.LessonId)).AsEnumerable();
        await _context.BulkDeleteAsync(entriesToRemove);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<UserLessonOccurrence>> GetByUserIdAndBetweenDateAsync(int userId, DateTimeOffset beginDate, DateTimeOffset endDate)
    {
        return await _context.UserLessonOccurrences
            .Where(x => x.UserId == userId)
            .Where(x => x.StartTime >= beginDate && x.StartTime < endDate)
            .ToListAsync();
    }

    public async Task<UserLessonOccurrence?> GetLatestOccurrenceAsync(int lessonId)
    {
        return await _context.UserLessonOccurrences
            .Where(x => x.LessonId == lessonId)
            .OrderByDescending(x => x.StartTime)
            .FirstOrDefaultAsync();
    }
}