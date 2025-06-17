using WordWisp.Web.Models.Enums;

namespace WordWisp.Web.Models.DTOs.Materials
{
    public class MaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Content { get; set; }
        public MaterialType MaterialType { get; set; }
        public string? FilePath { get; set; }
        public string? ExternalUrl { get; set; }
        public long? FileSize { get; set; }
        public string? MimeType { get; set; }
        public string? OriginalFileName { get; set; }
        public bool IsPublic { get; set; }
        public int Order { get; set; }
        public int TopicId { get; set; }
        public string TopicTitle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
