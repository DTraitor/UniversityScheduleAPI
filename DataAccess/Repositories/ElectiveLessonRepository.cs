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

    public void RemoveByKey(int key)
    {
        _context.ElectiveLessons.RemoveRange(_context.ElectiveLessons.Where(x => x.ElectiveLessonDayId == key));
    }

    public async Task<ElectiveLesson?> GetByIdAsync(int id)
    {
        return await _context.ElectiveLessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ElectiveLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ElectiveLessons.ToListAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
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
        _context.ElectiveLessons.AddRange(lessons);
    }

    public void UpdateRange(IEnumerable<ElectiveLesson> entity)
    {
        _context.ElectiveLessons.UpdateRange(entity);
    }

    public void RemoveRange(IEnumerable<ElectiveLesson> lessons)
    {
        _context.ElectiveLessons.RemoveRange(lessons);
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

    public async Task<IEnumerable<ElectiveLesson>> GetByElectiveDayIdAsync(int electiveDayId)
    {
        return await _context.ElectiveLessons.Where(x => x.ElectiveLessonDayId == electiveDayId).ToListAsync();
    }

    public async Task<IEnumerable<ElectiveLesson>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.ElectiveLessons.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}