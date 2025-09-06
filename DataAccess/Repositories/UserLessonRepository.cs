using DataAccess.Domain;
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

    public async Task AddRangeAsync(IEnumerable<UserLesson> lessons)
    {
        await _context.UserLessons.AddRangeAsync(lessons);
    }

    public void ClearByUserId(int userId)
    {
        _context.UserLessons.RemoveRange(_context.UserLessons.Where(x => x.UserId == userId));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserLesson>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.UserLessons.Where(ul => ids.Contains(ul.Id)).ToListAsync();
    }

    public IEnumerable<UserLesson> GetWithOccurrencesCalculatedDateLessThan(DateTimeOffset dateTime)
    {
        return _context.UserLessons.Where(l => l.OccurrencesCalculatedTill < dateTime).ToList();
    }
}