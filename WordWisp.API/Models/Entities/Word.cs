namespace WordWisp.API.Entities
{
    public class Word
    {
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string? Transcription { get; set; }
        public string? Example { get; set; }
        public int DictionaryId { get; set; }

        public Dictionary Dictionary { get; set; } = null!;
    }
}
