using LanguageLearning.Domain.Common;
using LanguageLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckTag> DeckTags => Set<DeckTag>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CardSchedule> CardSchedules => Set<CardSchedule>();
    public DbSet<ReviewEvent> ReviewEvents => Set<ReviewEvent>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<DailyLearningStats> DailyLearningStats => Set<DailyLearningStats>();
    public DbSet<UserGoal> UserGoals => Set<UserGoal>();
    public DbSet<SyncOperation> SyncOperations => Set<SyncOperation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Card>().Property(card => card.Version).IsConcurrencyToken();
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");
        modelBuilder.Entity<Language>().ToTable("languages");
        modelBuilder.Entity<Course>().ToTable("courses");
        modelBuilder.Entity<Unit>().ToTable("units");
        modelBuilder.Entity<Lesson>().ToTable("lessons");
        modelBuilder.Entity<Vocabulary>().ToTable("vocabularies");
        modelBuilder.Entity<Deck>().ToTable("decks");
        modelBuilder.Entity<DeckTag>().ToTable("deck_tags");
        modelBuilder.Entity<Card>().ToTable("cards");
        modelBuilder.Entity<CardSchedule>().ToTable("card_schedules");
        modelBuilder.Entity<ReviewEvent>().ToTable("review_events");
        modelBuilder.Entity<Question>().ToTable("questions");
        modelBuilder.Entity<QuestionOption>().ToTable("question_options");
        modelBuilder.Entity<QuizAttempt>().ToTable("quiz_attempts");
        modelBuilder.Entity<QuizAnswer>().ToTable("quiz_answers");
        modelBuilder.Entity<LessonProgress>().ToTable("lesson_progresses");
        modelBuilder.Entity<CourseEnrollment>().ToTable("course_enrollments");
        modelBuilder.Entity<DailyLearningStats>().ToTable("daily_learning_stats");
        modelBuilder.Entity<UserGoal>().ToTable("user_goals");
        modelBuilder.Entity<SyncOperation>().ToTable("sync_operations");
        modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<DeckTag>()
            .HasIndex(dt => new { dt.DeckId, dt.Tag })
            .IsUnique();

        modelBuilder.Entity<CardSchedule>()
            .HasIndex(cs => new { cs.UserId, cs.CardId })
            .IsUnique();

        modelBuilder.Entity<ReviewEvent>()
            .HasIndex(re => new { re.UserId, re.ClientEventId })
            .IsUnique();

        modelBuilder.Entity<LessonProgress>()
            .HasIndex(lp => new { lp.UserId, lp.LessonId })
            .IsUnique();

        modelBuilder.Entity<CourseEnrollment>()
            .HasIndex(ce => new { ce.UserId, ce.CourseId })
            .IsUnique();

        modelBuilder.Entity<DailyLearningStats>()
            .HasIndex(dls => new { dls.UserId, dls.LocalDate })
            .IsUnique();

        modelBuilder.Entity<SyncOperation>()
            .HasIndex(so => new { so.UserId, so.ClientOperationId })
            .IsUnique();

        modelBuilder.Entity<CardSchedule>()
            .HasOne(cs => cs.User)
            .WithMany(u => u.CardSchedules)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReviewEvent>()
            .HasOne(re => re.User)
            .WithMany(u => u.ReviewEvents)
            .HasForeignKey(re => re.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReviewEvent>()
            .HasOne(re => re.Card)
            .WithMany(c => c.ReviewEvents)
            .HasForeignKey(re => re.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LessonProgress>()
            .HasOne(lp => lp.User)
            .WithMany(u => u.LessonProgresses)
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.User)
            .WithMany(u => u.CourseEnrollments)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DailyLearningStats>()
            .HasOne(dls => dls.User)
            .WithMany(u => u.DailyLearningStats)
            .HasForeignKey(dls => dls.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SyncOperation>()
            .HasOne(so => so.User)
            .WithMany(u => u.SyncOperations)
            .HasForeignKey(so => so.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    private void PrepareForSave()
    {
        var reviewEvents = ChangeTracker.Entries<ReviewEvent>();

        foreach (var entry in reviewEvents)
        {
            if (entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                throw new InvalidOperationException(
                    "Review events are append-only and cannot be modified or deleted.");
            }
        }

        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    public override int SaveChanges()
    {
        PrepareForSave();
        return base.SaveChanges(true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareForSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        PrepareForSave();
        return base.SaveChangesAsync(true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepareForSave();
        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

}
