using Microsoft.EntityFrameworkCore;
using WordWisp.API.Models.Entities;
using WordWisp.API.Extensions;
using WordWisp.API.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Dictionary>().ToTable("dictionaries");
            modelBuilder.Entity<Word>().ToTable("words");

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
        }
    }
}
