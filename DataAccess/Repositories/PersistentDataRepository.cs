using Common.Models;
using DataAccess.Domain;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataAccess.Repositories;

public class PersistentDataRepository : IPersistentDataRepository
{
    private readonly ScheduleDbContext _context;
    private readonly ILogger<PersistentDataRepository> _logger;

    public PersistentDataRepository(ScheduleDbContext scheduleDbContext, ILogger<PersistentDataRepository> logger)
    {
        _context = scheduleDbContext;
        _logger = logger;
    }

    public void SetData(PersistentData persistentData)
    {
        var existing = _context.PersistentData.FirstOrDefault(x => x.Key == persistentData.Key);

        if (existing == null)
        {
            _context.PersistentData.Add(persistentData);
        }
        else
        {
            persistentData.Id = existing.Id;
            _context.Entry(existing).CurrentValues.SetValues(persistentData);
        }
    }

    public async Task<PersistentData?> GetDataAsync(string key)
    {
        return await _context.PersistentData.FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}