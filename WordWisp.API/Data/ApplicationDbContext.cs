using Microsoft.EntityFrameworkCore;
using WordWisp.API.Models.Entities;
using WordWisp.API.Extensions;
using WordWisp.API.Entities;
using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Dictionary> Dictionaries { get; set; }
        public DbSet<Word> Words { get; set; }

        public DbSet<LevelTest> LevelTests { get; set; }
        public DbSet<LevelTestQuestion> LevelTestQuestions { get; set; }
        public DbSet<LevelTestAnswer> LevelTestAnswers { get; set; }

        public DbSet<ReadingPassage> ReadingPassages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Dictionary>().ToTable("dictionaries");
            modelBuilder.Entity<Word>().ToTable("words");
            modelBuilder.Entity<LevelTest>().ToTable("level_tests");
            modelBuilder.Entity<LevelTestQuestion>().ToTable("level_test_questions");
            modelBuilder.Entity<LevelTestAnswer>().ToTable("level_test_answers");
            modelBuilder.Entity<ReadingPassage>().ToTable("reading_passages");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }
            }

            base.OnModelCreating(modelBuilder);

            // User Configuration

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Dictionary Configuration

            modelBuilder.Entity<Dictionary>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Description).HasMaxLength(500);
                entity.Property(d => d.IsPublic).HasDefaultValue(false);
                entity.Property(d => d.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(d => d.UpdatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Word Configuration

            modelBuilder.Entity<Word>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Term).IsRequired().HasMaxLength(200);
                entity.Property(w => w.Definition).IsRequired().HasMaxLength(500);
                entity.Property(w => w.Transcription).HasMaxLength(200);

                entity.HasOne(w => w.Dictionary)
                      .WithMany(d => d.Words)
                      .HasForeignKey(w => w.DictionaryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LevelTest Configuration

            modelBuilder.Entity<LevelTest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.StartedAt).IsRequired();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.DeterminedLevel).HasConversion<int?>();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.Status });
            });

            modelBuilder.Entity<LevelTestQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.OptionA).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OptionB).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OptionC).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OptionD).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CorrectAnswer).IsRequired().HasMaxLength(1);
                entity.Property(e => e.Section).HasConversion<int>();
                entity.Property(e => e.Difficulty).HasConversion<int>();

                entity.HasIndex(e => e.Section);
                entity.HasIndex(e => new { e.Section, e.IsActive });

                entity.HasOne(e => e.ReadingPassage)
                  .WithMany(p => p.Questions)
                  .HasForeignKey(e => e.ReadingPassageId)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<LevelTestAnswer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SelectedAnswer).IsRequired().HasMaxLength(1);

                entity.Property(e => e.QuestionDifficulty).HasConversion<int>();
                entity.Property(e => e.EstimatedUserLevel).HasConversion<int>();

                entity.HasOne(e => e.LevelTest)
                      .WithMany(e => e.Answers)
                      .HasForeignKey(e => e.LevelTestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Question)
                      .WithMany()
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.LevelTestId);
                entity.HasIndex(e => new { e.LevelTestId, e.QuestionId }).IsUnique();
                entity.HasIndex(e => new { e.LevelTestId, e.QuestionOrder });
            });

            // LevelTest - ReadingPassage

            modelBuilder.Entity<ReadingPassage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Topic).HasMaxLength(100);
                entity.Property(e => e.Level).HasConversion<int>();

                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => new { e.Level, e.IsActive });
            });
        }
    }
}
