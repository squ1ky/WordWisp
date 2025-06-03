namespace WordWisp.API.Models.DTOs.Words
{
    public class WordDto
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? Transcription { get; set; }
        public string? Example { get; set; }
    }
}

