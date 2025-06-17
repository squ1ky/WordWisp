using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WordWisp.API.Entities;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.Entities
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }

        // Настройки упражнения
        public int TimeLimit { get; set; } = 30; // в минутах
        public int MaxAttempts { get; set; } = 3; // максимальное количество попыток
        public int PassingScore { get; set; } = 70; // проходной балл в процентах
        public bool IsActive { get; set; } = true;
        public int Order { get; set; } = 0;

        // Связь с материалом (делаем опциональной)
        public int? MaterialId { get; set; }
        [ForeignKey(nameof(MaterialId))]
        public Material? Material { get; set; }

        // Добавляем прямую связь с темой
        [Required]
        public int TopicId { get; set; }
        [ForeignKey(nameof(TopicId))]
        public Topic Topic { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Связи
        public ICollection<ExerciseQuestion> Questions { get; set; } = new List<ExerciseQuestion>();
        public ICollection<UserExerciseAttempt> UserAttempts { get; set; } = new List<UserExerciseAttempt>();
    }
}
