using System.ComponentModel.DataAnnotations.Schema;
using WordWisp.API.Models.Entities;

namespace WordWisp.API.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; } = true;
        public int CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public User CreatedByUser { get; set; } = null!;
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}