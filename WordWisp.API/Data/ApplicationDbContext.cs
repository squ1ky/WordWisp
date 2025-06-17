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
        public DbSet<LevelTest> LevelTests { get; set;}
        public DbSet<LevelTestQuestion> LevelTestQuestions { get; set; }
        public DbSet<LevelTestAnswer> LevelTestAnswers{ get; set; }
        public DbSet<ReadingPassage> ReadingPassages{ get; set; }
        public DbSet<Topic> Topics{ get; set; }
        public DbSet<Material> Materials{ get; set; }
        public DbSet<Exercise> Exercises{ get; set; }
        public DbSet<ExerciseQuestion> ExerciseQuestions{ get; set; }
        public DbSet<ExerciseAnswer> ExerciseAnswers{ get; set; }
        public DbSet<UserExerciseAttempt> UserExerciseAttempts{ get; set; }
        public DbSet<UserExerciseAnswer> UserExerciseAnswers{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Dictionary>().ToTable("dictionaries");
            modelBuilder.Entity<Word>().ToTable("words");
            modelBuilder.Entity<LevelTest>().ToTable("level_tests");
            modelBuilder.Entity<LevelTestQuestion>().ToTable("level_test_questions");
            modelBuilder.Entity<LevelTestAnswer>().ToTable("level_test_answers");
            modelBuilder.Entity<ReadingPassage>().ToTable("reading_passages");
            modelBuilder.Entity<Topic>().ToTable("topics");
            modelBuilder.Entity<Material>().ToTable("materials");
            modelBuilder.Entity<Exercise>().ToTable("exercises");
            modelBuilder.Entity<ExerciseQuestion>().ToTable("exercise_questions");
            modelBuilder.Entity<ExerciseAnswer>().ToTable("exercise_answers");
            modelBuilder.Entity<UserExerciseAttempt>().ToTable("user_exercise_attempts");
            modelBuilder.Entity<UserExerciseAnswer>().ToTable("user_exercise_answers");

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }
            }

            base.OnModelCreating(modelBuilder);

            // Topics Configuration
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");

                entity.HasOne(t => t.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedBy)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Materials Configuration
            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(m => m.Description)
                    .HasMaxLength(1000);

                entity.Property(m => m.FilePath)
                    .HasMaxLength(500);

                entity.Property(m => m.ExternalUrl)
                    .HasMaxLength(1000);

                entity.Property(m => m.MimeType)
                    .HasMaxLength(100);

                entity.Property(m => m.OriginalFileName)
                    .HasMaxLength(255);

                entity.Property(m => m.MaterialType)
                    .HasConversion<int>();

                entity.Property(m => m.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(m => m.Topic)
                    .WithMany(t => t.Materials)
                    .HasForeignKey(m => m.TopicId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Exercises Configuration
            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);

                // ИЗМЕНЕНО: Обновляем тип enum
                entity.Property(e => e.ExerciseType).IsRequired().HasConversion<string>();

                // ДОБАВЛЕНО: Новые поля
                entity.Property(e => e.TimeLimit).HasDefaultValue(30);
                entity.Property(e => e.MaxAttempts).HasDefaultValue(3);
                entity.Property(e => e.PassingScore).HasDefaultValue(70);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Order).HasDefaultValue(0);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt); // Без значения по умолчанию

                // ИЗМЕНЕНО: MaterialId теперь опциональный
                entity.HasOne(e => e.Material)
                    .WithMany(m => m.Exercises)
                    .HasForeignKey(e => e.MaterialId)
                    .OnDelete(DeleteBehavior.SetNull); // ИЗМЕНЕНО: SetNull вместо Cascade

                // ДОБАВЛЕНО: Прямая связь с Topic
                entity.HasOne(e => e.Topic)
                    .WithMany(t => t.Exercises)
                    .HasForeignKey(e => e.TopicId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Exercise Questions Configuration
            modelBuilder.Entity<ExerciseQuestion>(entity =>
            {
                entity.HasKey(eq => eq.Id);

                entity.Property(eq => eq.Question)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(eq => eq.QuestionImagePath)
                    .HasMaxLength(500);

                entity.Property(eq => eq.QuestionAudioPath)
                    .HasMaxLength(500);

                entity.Property(eq => eq.Order)
                    .HasDefaultValue(0);

                entity.Property(eq => eq.Points)
                    .HasDefaultValue(1);

                entity.Property(eq => eq.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(eq => eq.Exercise)
                    .WithMany(e => e.Questions)
                    .HasForeignKey(eq => eq.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Exercise Answers Configuration
            modelBuilder.Entity<ExerciseAnswer>(entity =>
            {
                entity.HasKey(ea => ea.Id);

                entity.Property(ea => ea.AnswerText)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(ea => ea.AnswerImagePath)
                    .HasMaxLength(500);

                entity.Property(ea => ea.IsCorrect)
                    .HasDefaultValue(false);

                entity.Property(ea => ea.Order)
                    .HasDefaultValue(0);

                entity.Property(ea => ea.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(ea => ea.Question)
                    .WithMany(eq => eq.Answers)
                    .HasForeignKey(ea => ea.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User Exercise Attempts Configuration
            modelBuilder.Entity<UserExerciseAttempt>(entity =>
            {
                entity.HasKey(uea => uea.Id);
                entity.Property(uea => uea.StartedAt).HasDefaultValueSql("NOW()");
                entity.Property(uea => uea.Score).HasDefaultValue(0);
                entity.Property(uea => uea.MaxPossibleScore).HasDefaultValue(100);
                entity.Property(uea => uea.IsCompleted).HasDefaultValue(false);
                entity.Property(uea => uea.IsPassed).HasDefaultValue(false);
                entity.Property(uea => uea.TimeSpentSeconds).HasDefaultValue(0);

                // ОБНОВЛЕНО: Связь с Exercise через UserAttempts
                entity.HasOne(uea => uea.Exercise)
                    .WithMany(e => e.UserAttempts) // ДОБАВЛЕНО: навигационное свойство
                    .HasForeignKey(uea => uea.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uea => uea.User)
                    .WithMany()
                    .HasForeignKey(uea => uea.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User Exercise Answers Configuration
            modelBuilder.Entity<UserExerciseAnswer>(entity =>
            {
                entity.HasKey(uea => uea.Id);

                entity.Property(uea => uea.AnswerText)
                    .HasMaxLength(1000);

                entity.Property(uea => uea.PointsEarned)
                    .HasPrecision(5, 2)
                    .HasDefaultValue(0);

                entity.Property(uea => uea.AnsweredAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(uea => uea.UserAttempt)
                    .WithMany(ua => ua.UserAnswers)
                    .HasForeignKey(uea => uea.UserAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uea => uea.Question)
                    .WithMany(q => q.UserAnswers)
                    .HasForeignKey(uea => uea.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict); // Изменено на Restrict для сохранения истории

                // Связь с выбранным ответом (для множественного выбора)
                entity.HasOne<ExerciseAnswer>()
                    .WithMany()
                    .HasForeignKey(uea => uea.SelectedAnswerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Индекс для быстрого поиска ответов по попытке
                entity.HasIndex(uea => uea.UserAttemptId)
                    .HasDatabaseName("IX_UserExerciseAnswer_UserAttempt");
            });

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
