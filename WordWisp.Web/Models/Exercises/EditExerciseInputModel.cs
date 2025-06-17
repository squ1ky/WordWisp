using System.ComponentModel.DataAnnotations;
using WordWisp.Web.Models.Enums;

namespace WordWisp.Web.Models.Exercises
{
    public class EditExerciseInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Тип упражнения обязателен")]
        public ExerciseType ExerciseType { get; set; }

        public int? MaterialId { get; set; }

        [Range(1, 300, ErrorMessage = "Время должно быть от 1 до 300 минут")]
        public int TimeLimit { get; set; } = 30;

        [Range(1, 10, ErrorMessage = "Количество попыток должно быть от 1 до 10")]
        public int MaxAttempts { get; set; } = 3;

        [Range(0, 100, ErrorMessage = "Проходной балл должен быть от 0 до 100")]
        public int PassingScore { get; set; } = 70;

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Порядок должен быть положительным")]
        public int Order { get; set; } = 0;

        public List<EditQuestionInputModel> Questions { get; set; } = new();
    }

    public class EditQuestionInputModel
    {
        public int? Id { get; set; } // null для новых вопросов

        [Required(ErrorMessage = "Текст вопроса обязателен")]
        [StringLength(1000, ErrorMessage = "Вопрос не должен превышать 1000 символов")]
        public string Question { get; set; } = "";

        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }

        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int Points { get; set; } = 1;

        [Range(0, int.MaxValue, ErrorMessage = "Порядок должен быть положительным")]
        public int Order { get; set; }

        public List<EditAnswerInputModel> Answers { get; set; } = new();
    }

    public class EditAnswerInputModel
    {
        public int? Id { get; set; } // null для новых ответов

        [Required(ErrorMessage = "Текст ответа обязателен")]
        [StringLength(500, ErrorMessage = "Ответ не должен превышать 500 символов")]
        public string AnswerText { get; set; } = "";

        public string? AnswerImagePath { get; set; }
        public bool IsCorrect { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Порядок должен быть положительным")]
        public int Order { get; set; }
    }
}
