namespace WordWisp.API.Models.DTOs.LevelTest
{
    public class SubmitAnswerRequest
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; } = "";
    }
}

