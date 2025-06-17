using WordWisp.Web.Models.Enums;

namespace WordWisp.Web.Models.DTOs.Exercises
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = "";
        public int? MaterialId { get; set; }
        public string? MaterialTitle { get; set; }
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public bool IsActive { get; set; }
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // ДОБАВЬТЕ ЭТО СВОЙСТВО
        public List<ExerciseQuestionDto> Questions { get; set; } = new();
    }

    // Остальные классы остаются без изменений
    public class ExerciseQuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = "";
        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }
        public int Order { get; set; }
        public int Points { get; set; }
        public List<ExerciseAnswerDto> Answers { get; set; } = new();
    }

    public class ExerciseAnswerDto
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = "";
        public string? AnswerImagePath { get; set; }
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    // DTO для студентов (без правильных ответов)
    public class StudentExerciseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = "";
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public int PassingScore { get; set; }
        public List<StudentExerciseQuestionDto> Questions { get; set; } = new();
    }

    public class StudentExerciseQuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = "";
        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }
        public int Order { get; set; }
        public int Points { get; set; }
        public List<StudentExerciseAnswerDto> Answers { get; set; } = new();
    }

    public class StudentExerciseAnswerDto
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } = "";
        public string? AnswerImagePath { get; set; }
        public int Order { get; set; }
        // НЕ включаем IsCorrect для студентов
    }
}
