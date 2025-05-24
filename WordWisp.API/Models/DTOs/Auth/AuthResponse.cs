using WordWisp.API.Models.Entities;

namespace WordWisp.API.Models.DTOs.Auth
{
    public class AuthResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string Token { get; set; }
    }
}
