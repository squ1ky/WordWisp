using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Requests.Exercises.Student
{
    public class StartExerciseRequest
    {
        [Required]
        public int ExerciseId { get; set; }
    }

    public class SubmitExerciseRequest
    {
        [Required]
        public int ExerciseId { get; set; }

        [Required]
        public int AttemptId { get; set; }

        public List<SubmitExerciseAnswerRequest> Answers { get; set; } = new();
    }

    public class SubmitExerciseAnswerRequest
    {
        [Required]
        public int QuestionId { get; set; }

        public List<int> SelectedAnswerIds { get; set; } = new(); // Для multiple choice
        public string? TextAnswer { get; set; } // Для fill-in-blank, essay
    }
}
