using WordWisp.Web.Models.Entities;

namespace WordWisp.Web.Models.DTOs.Users
{
    public class UserStatsDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Email { get; set; } = "";
        public UserRole Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        // Реальная статистика
        public int DictionariesCount { get; set; }

        // Заглушки для студента
        public string? EnglishLevel { get; set; } // "Beginner", "Intermediate", etc. или null
        public DateTime? LastTestDate { get; set; }
        public int StudiedWordsCount { get; set; } // Заглушка

        // Заглушки для преподавателя
        public int CreatedMaterialsCount { get; set; } // Заглушка
        public int StudentsViewedCount { get; set; } // Заглушка
    }
}
