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

    public void RemoveRange(IEnumerable<ElectiveLesson> lessons)
    {
        _context.ElectiveLessons.RemoveRange(lessons);
    }

    public void RemoveAll()
    {
        _context.ElectiveLessons.RemoveRange(_context.ElectiveLessons);
    }
}