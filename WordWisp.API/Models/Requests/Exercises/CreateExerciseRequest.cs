using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.Requests.Exercises.Teacher
{
    public class CreateExerciseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }

        [Required]
        public int TopicId { get; set; }

        public int? MaterialId { get; set; } // Опционально

        [Range(1, 300)]
        public int TimeLimit { get; set; } = 30;

        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 3;

        [Range(0, 100)]
        public int PassingScore { get; set; } = 70;

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int Order { get; set; } = 0;

        public List<CreateQuestionRequest> Questions { get; set; } = new();
    }

    public class CreateQuestionRequest
    {
        [Required]
        [StringLength(1000)]
        public string Question { get; set; } = "";

        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Range(1, 100)]
        public int Points { get; set; } = 1;

        public List<CreateAnswerRequest> Answers { get; set; } = new();
    }

    public class CreateAnswerRequest
    {
        [Required]
        [StringLength(500)]
        public string AnswerText { get; set; } = "";

        public string? AnswerImagePath { get; set; }
        public bool IsCorrect { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }
    }
}
