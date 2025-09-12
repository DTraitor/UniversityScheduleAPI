using DataAccess.Domain;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class UserModifiedRepository : IUserModifiedRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<UserModifiedRepository> _logger;

    public UserModifiedRepository(ScheduleDbContext scheduleDbContext, ILogger<UserModifiedRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<UserModified>> GetNotProcessed(ProcessedByEnum flag, CancellationToken cancellationToken = default)
    {
        return await _context.UserModifications.Where(x => (x.ProcessedBy & flag) != ProcessedByEnum.None).ToListAsync(cancellationToken);
    }

    public async Task ActivateBitFlag(int id, ProcessedByEnum flag)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE UserModifications SET Flags = Flags | {(int)flag} WHERE Id = {id}");
    }

    public async Task DeactivateBitFlag(int id, ProcessedByEnum flag)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE UserModifications SET Flags = Flags & ~{(int)flag} WHERE Id = {id}");
    }
}