using DataAccess.Domain;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
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
        var existing = _context.PersistentData.Find(persistentData.Id);

        if (existing == null)
        {
            _context.PersistentData.Add(persistentData);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(persistentData);
        }
    }

    public PersistentData GetData()
    {
        return _context.PersistentData.FirstOrDefault() ?? new PersistentData{ Id = 0 };
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}