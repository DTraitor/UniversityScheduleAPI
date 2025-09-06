using DataAccess.Domain;
using DataAccess.Models.Internal;
using DataAccess.Repositories.Interfaces;
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

    public async Task AddRangeAsync(IEnumerable<UserLessonOccurrence> lessonOccurences)
    {
        await _context.UserLessonOccurrences.AddRangeAsync(lessonOccurences);
    }

    public void ClearByUserId(int userId)
    {
        _context.UserLessonOccurrences.RemoveRange(_context.UserLessonOccurrences.Where(x => x.UserId == userId));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}