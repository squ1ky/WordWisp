using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.DTOs.Materials
{
    public class PublicMaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Content { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? ExternalUrl { get; set; }
        public string? OriginalFileName { get; set; }
        public int Order { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public int ExercisesCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
