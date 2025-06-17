using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.Requests.Exercises.Teacher
{
    public class UpdateExerciseRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public ExerciseType ExerciseType { get; set; }

        public int? MaterialId { get; set; }

        [Range(1, 300)]
        public int TimeLimit { get; set; }

        [Range(1, 10)]
        public int MaxAttempts { get; set; }

        [Range(0, 100)]
        public int PassingScore { get; set; }

        public bool IsActive { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        public List<UpdateQuestionRequest> Questions { get; set; } = new();
    }

    public class UpdateQuestionRequest
    {
        public int? Id { get; set; } // null для новых вопросов

        [Required]
        [StringLength(1000)]
        public string Question { get; set; } = "";

        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }

        [Range(1, 100)]
        public int Points { get; set; }

        public List<UpdateAnswerRequest> Answers { get; set; } = new();
    }

    public class UpdateAnswerRequest
    {
        public int? Id { get; set; } // null для новых ответов

        [Required]
        [StringLength(500)]
        public string AnswerText { get; set; } = "";

        public string? AnswerImagePath { get; set; }
        public bool IsCorrect { get; set; }

        [Range(0, int.MaxValue)]
        public int Order { get; set; }
    }
}
