using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
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
            _context.Entry(existing).CurrentValues.SetValues(persistentData);
        }
    }

    public async Task<PersistentData?> GetData(string key)
    {
        return await _context.PersistentData.FirstOrDefaultAsync(x => x.Key == key);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}