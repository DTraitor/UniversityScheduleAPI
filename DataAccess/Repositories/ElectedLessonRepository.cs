using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ElectedLessonRepository : IElectedLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ElectedLessonRepository> _logger;

    public ElectedLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<ElectedLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(ElectedLesson entity)
    {
        _context.ElectedLessons.Add(entity);
    }

    public void Update(ElectedLesson entity)
    {
        _context.ElectedLessons.Update(entity);
    }

    public void Delete(ElectedLesson entity)
    {
        _context.ElectedLessons.Remove(entity);
    }

    public void AddRange(IEnumerable<ElectedLesson> entities)
    {
        _context.FutureAction(x => x.BulkInsert(entities));
    }

    public void UpdateRange(IEnumerable<ElectedLesson> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<ElectedLesson> entities)
    {
        _context.FutureAction(x => x.BulkDelete(entities));
    }

    public async Task<ElectedLesson?> GetByIdAsync(int id)
    {
        return await _context.ElectedLessons.FirstOrDefaultAsync(x =>  x.Id == id);
    }

    public async Task<IEnumerable<ElectedLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectedLessons.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        _context.ExecuteFutureAction();
        _context.SaveChanges();
    }

    public async Task<IEnumerable<ElectedLesson>> GetByUserId(int userId)
    {
        return await _context.ElectedLessons.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<IEnumerable<ElectedLesson>> GetByUserIds(IEnumerable<int> userIds)
    {
        return await _context.ElectedLessons.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<IEnumerable<ElectedLesson>> GetByElectiveDayIdsAsync(IEnumerable<int> electiveDayIds)
    {
        return await _context.ElectedLessons.Where(x => electiveDayIds.Contains(x.ElectiveLessonDayId)).ToListAsync();
    }

    public async Task<IEnumerable<ElectedLesson>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ElectedLessons.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<IEnumerable<ElectedLesson>> GetByElectiveLessonIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ElectedLessons.Where(x =>  ids.Contains(x.ElectiveLessonId)).ToListAsync();
    }
}