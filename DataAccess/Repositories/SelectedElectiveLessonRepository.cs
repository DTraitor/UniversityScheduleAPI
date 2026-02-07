using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class SelectedElectiveLessonRepository : ISelectedElectiveLesson
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<SelectedElectiveLessonRepository> _logger;

    public SelectedElectiveLessonRepository(ScheduleDbContext scheduleDbContext, ILogger<SelectedElectiveLessonRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Add(entity);
    }

    public void Update(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Update(entity);
    }

    public void Delete(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessonEntries.Remove(entity);
    }

    public async Task AddRangeAsync(ICollection<SelectedElectiveLesson> toAdd)
    {
        await _context.BulkInsertAsync(toAdd);
    }

    public async Task UpdateRangeAsync(ICollection<SelectedElectiveLesson> entity)
    {
        await _context.BulkUpdateAsync(entity);
    }

    public async Task RemoveRangeAsync(ICollection<SelectedElectiveLesson> entities)
    {
        await _context.BulkDeleteAsync(entities);
    }

    public async Task<SelectedElectiveLesson?> GetByIdAsync(int id)
    {
        return await _context.SelectedElectiveLessonEntries.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedElectiveLessonEntries.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetBySourceIds(ICollection<int> sourceIds)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => sourceIds.Contains(x.LessonSourceId)).ToListAsync();
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetByUserIds(ICollection<int> userIds)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetByUserId(int userId)
    {
        return await _context.SelectedElectiveLessonEntries.Where(x => x.UserId == userId).ToListAsync();
    }
}