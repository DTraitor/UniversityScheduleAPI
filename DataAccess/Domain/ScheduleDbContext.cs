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

    public DbSet<UsageMetric> UsageMetrics { get; set; }

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

        modelBuilder.Entity<UserModified>()
            .HasIndex(e => new { e.Id, e.ToProcess });

        modelBuilder.Entity<UserAlert>()
            .HasIndex(e => e.Id);

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Id);

        modelBuilder.Entity<UserLessonOccurrence>()
            .HasIndex(e => new { e.LessonId, e.UserId });

        modelBuilder.Entity<UserLesson>()
            .HasIndex(e => new { e.Id, e.UserId });

        modelBuilder.Entity<GroupLesson>()
            .HasIndex(e => new { e.Id, e.GroupId });

        modelBuilder.Entity<ElectiveLesson>()
            .HasIndex(e => new { e.Id, e.ElectiveLessonDayId });

        base.OnModelCreating(modelBuilder);
    }
}