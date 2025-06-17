using WordWisp.API.Models.Entities;

namespace WordWisp.API.Models.DTOs.Topics
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int MaterialsCount { get; set; }
        public int ExercisesCount { get; set; }
    }
}