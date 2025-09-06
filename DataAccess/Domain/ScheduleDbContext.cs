using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Domain;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    // University Schedule
    public DbSet<ScheduleLesson> ScheduleLessons { get; set; }
    public DbSet<ElectiveLesson> ElectiveLessons { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Group> RemovedGroups { get; set; }

    // Internal Schedule
    public DbSet<UserLesson> UserLessons { get; set; }
    public DbSet<UserLessonOccurrence> UserLessonOccurrences { get; set; }

    // Internal stuff
    public DbSet<PersistentData> PersistentData { get; set; }
}