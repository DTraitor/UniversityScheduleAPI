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
        persistentData.Id = GetData().Id;
        _context.PersistentData.Add(persistentData);
    }

    public PersistentData GetData()
    {
        return _context.PersistentData.FirstOrDefault(new PersistentData() { Id = 0 });
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }
}