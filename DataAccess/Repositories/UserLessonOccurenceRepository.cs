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

    public void AddRange(IEnumerable<UserLessonOccurrence> lessonOccurrences)
    {
        _context.UserLessonOccurrences.AddRange(lessonOccurrences);
    }

    public void ClearByUserId(int userId)
    {
        _context.UserLessonOccurrences.RemoveRange(_context.UserLessonOccurrences.Where(x => x.UserId == userId));
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserLessonOccurrence>> GetByUserIdAsync(int userId)
    {
        return await _context.UserLessonOccurrences.Where(x => x.UserId == userId).ToListAsync();
    }

    public UserLessonOccurrence? GetLatestOccurrence(int lessonId)
    {
        return _context.UserLessonOccurrences
            .Where(x => x.LessonId == lessonId)
            .OrderByDescending(x => x.StartTime)
            .FirstOrDefault();
    }
}