using DataAccess.Domain;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
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
        _context.UserLessonOccurrences.AddRange(lessonOccurrences);
    }

    public void RemoveRange(IEnumerable<UserLessonOccurrence> entities)
    {
        _context.UserLessonOccurrences.RemoveRange(entities);
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
        _context.UserLessonOccurrences.RemoveRange(_context.UserLessonOccurrences.Where(x => toRemove.Contains(x.LessonId)));
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAndDateAsync(int userId, DateTimeOffset date)
    {
        return await _context.UserLessonOccurrences
            .Where(x => x.UserId == userId)
            .Where(x => x.StartTime >= date.Date && x.StartTime < date.Date.AddDays(1))
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