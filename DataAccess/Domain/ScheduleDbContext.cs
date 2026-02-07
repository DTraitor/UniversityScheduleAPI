using Common.Models;
using Common.Models.Internal;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Domain;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    //Lesson sources
    public DbSet<LessonSource> LessonSources { get; set; }
    public DbSet<LessonEntry> LessonEntries { get; set; }

    //Selected
    public DbSet<SelectedLessonSource> SelectedLessonSources { get; set; }
    public DbSet<SelectedElectiveLesson> SelectedElectiveLessonEntries { get; set; }

    //Modifications
    public DbSet<UserModified> UserModifications { get; set; }
    public DbSet<LessonSourceModified> LessonSourceModifications { get; set; }

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

        modelBuilder.Entity<UserModified>()
            .HasIndex(e => e.Id );

        modelBuilder.Entity<UserAlert>()
            .HasIndex(e => e.Id);

        modelBuilder.Entity<User>()
            .HasIndex(e => e.Id);

        modelBuilder.Entity<User>()
            .HasIndex(e => e.TelegramId);

        modelBuilder.Entity<UserLessonOccurrence>()
            .HasIndex(e => new { e.LessonId, e.UserId });

        modelBuilder.Entity<UserLesson>()
            .HasIndex(e => new { e.OccurrencesCalculatedTill });

        modelBuilder.Entity<UserLesson>()
            .HasIndex(e => new { e.RepeatType });

        modelBuilder.Entity<UserLessonOccurrence>()
            .HasIndex(e => e.UserId );

        modelBuilder.Entity<UserLessonOccurrence>()
            .HasIndex(e => e.LessonId );

        modelBuilder.Entity<UserLesson>()
            .HasIndex(e => e.UserId );

        base.OnModelCreating(modelBuilder);
    }
}