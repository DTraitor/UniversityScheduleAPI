using DataAccess.Repositories.Interfaces;
using DataAccess.Domain;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class ElectiveLessonRepository : IElectiveLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<ElectiveLessonRepository> _logger;

    public ElectiveLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<ElectiveLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public async Task<ElectiveLesson?> GetByIdAsync(int id)
    {
        return await _context.ElectiveLessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ElectiveLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectiveLessons.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        _context.ExecuteFutureAction();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        _context.ExecuteFutureAction();
        _context.SaveChanges();
    }

    public void Add(ElectiveLesson entity)
    {
        _context.ElectiveLessons.Add(entity);
    }

    public void Update(ElectiveLesson entity)
    {
        _context.ElectiveLessons.Update(entity);
    }

    public void Delete(ElectiveLesson entity)
    {
        _context.ElectiveLessons.Remove(entity);
    }

    public void AddRange(IEnumerable<ElectiveLesson> lessons)
    {
        _context.FutureAction(x => x.BulkInsert(lessons));
    }

    public void UpdateRange(IEnumerable<ElectiveLesson> entity)
    {
        _context.FutureAction(x => x.BulkUpdate(entity));
    }

    public void RemoveRange(IEnumerable<ElectiveLesson> lessons)
    {
        _context.FutureAction(x => x.BulkDelete(lessons));
    }

    public async Task<IEnumerable<int>> GetUniqueLessonDaysAsync()
    {
        return await _context.ElectiveLessons.GroupBy(x => x.ElectiveLessonDayId).Select(x => x.Key).ToListAsync();
    }

    public async Task<IEnumerable<ElectiveLesson>> GetByDayIdAndPartialNameAsync(int dayId, string partialName)
    {
        return await _context.ElectiveLessons
            .Where(x => x.ElectiveLessonDayId == dayId)
            .Where(x => x.Title.ToLower().Contains(partialName.ToLower()))
            .ToListAsync();
    }

    public async Task<IEnumerable<ElectiveLesson>> GetByElectiveDayIdsAsync(IEnumerable<int> electiveDayIds)
    {
        return await _context.ElectiveLessons.Where(x => electiveDayIds.Contains(x.ElectiveLessonDayId)).ToListAsync();
    }

    public async Task<IEnumerable<ElectiveLesson>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ElectiveLessons.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}