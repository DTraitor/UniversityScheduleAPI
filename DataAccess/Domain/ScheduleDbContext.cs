using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace DataAccess.Domain;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    // Group Schedule
    public DbSet<GroupLesson> GroupLessons { get; set; }
    public DbSet<Group> Groups { get; set; }

    //Elective schedule
    public DbSet<ElectiveLesson> ElectiveLessons { get; set; }
    public DbSet<ElectedLesson> ElectedLessons { get; set; }
    public DbSet<ElectiveLessonDay> ElectiveLessonDays { get; set; }

    //Modifications
    public DbSet<UserModified> UserModifications { get; set; }
    public DbSet<GroupLessonModified> GroupLessonModifications { get; set; }
    public DbSet<ElectiveLessonModified> ElectiveLessonModifications { get; set; }

    // Internal Schedule
    public DbSet<UserLesson> UserLessons { get; set; }
    public DbSet<UserLessonOccurrence> UserLessonOccurrences { get; set; }

    // Internal stuff
    public DbSet<PersistentData> PersistentData { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserAlert> UserAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserLesson>()
            .Property(e => e.BeginTime)
            .HasColumnType("interval");

        modelBuilder.Entity<GroupLesson>()
            .Property(e => e.StartTime)
            .HasColumnType("interval");

        modelBuilder.Entity<ElectiveLesson>()
            .Property(e => e.StartTime)
            .HasColumnType("interval");

        base.OnModelCreating(modelBuilder);
    }
}