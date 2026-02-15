using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class SelectedElectiveLessonRepositoryRepository : ISelectedElectiveLessonRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<SelectedElectiveLessonRepositoryRepository> _logger;

    public SelectedElectiveLessonRepositoryRepository(ScheduleDbContext scheduleDbContext, ILogger<SelectedElectiveLessonRepositoryRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void Add(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessons.Add(entity);
    }

    public void Update(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessons.Update(entity);
    }

    public void Delete(SelectedElectiveLesson entity)
    {
        _context.SelectedElectiveLessons.Remove(entity);
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
        return await _context.SelectedElectiveLessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SelectedElectiveLessons.ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetBySourceIds(ICollection<int> sourceIds)
    {
        return await _context.SelectedElectiveLessons.Where(x => sourceIds.Contains(x.LessonSourceId)).ToListAsync();
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetByUserIds(ICollection<int> userIds)
    {
        return await _context.SelectedElectiveLessons.Where(x => userIds.Contains(x.UserId)).ToListAsync();
    }

    public async Task<ICollection<SelectedElectiveLesson>> GetByUserId(int userId)
    {
        return await _context.SelectedElectiveLessons.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}