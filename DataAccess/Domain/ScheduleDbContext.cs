using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess.Domain;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<ScheduleLesson> ScheduleLessons { get; set; }
    public DbSet<ElectiveLesson> ElectiveLessons { get; set; }
    public DbSet<Group> Groups { get; set; }
}