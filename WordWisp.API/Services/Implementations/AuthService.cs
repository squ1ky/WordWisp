using WordWisp.API.Models.DTOs.Auth;
using WordWisp.API.Models.Entities;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WordWisp.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository,
                           IEmailService emailService,
                           IConfiguration configuration)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new ArgumentException("Пользователь с таким email уже существует");

            if (await _userRepository.ExistsByUsernameAsync(request.Username))
                throw new ArgumentException("Пользователь с таким username уже существует");

            var verificationCode = GenerateVerificationCode();
                
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
                EmailVerificationCode = verificationCode,
                EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15)
            };

            await _userRepository.CreateAsync(user);

            await _emailService.SendVerificationEmailAsync(user.Email, verificationCode, user.Name);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Role = user.Role,
                Token = null
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Неверный email или пароль");

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Email не подтвержден. Проверьте почту и подтвердите регистрацию.");

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Role = user.Role,
                Token = token
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            if (user.IsEmailVerified)
                throw new ArgumentException("Email уже подтвержден");

            if (user.EmailVerificationCode != request.VerificationCode)
                throw new ArgumentException("Неверный код верификации");

            if (user.EmailVerificationCodeExpiry < DateTime.UtcNow)
                throw new ArgumentException("Код верификации истек");

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationCodeExpiry = null;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            if (user.IsEmailVerified)
                throw new ArgumentException("Email уже подтвержден");

            var verificationCode = GenerateVerificationCode();
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userRepository.UpdateAsync(user);

            await _emailService.SendVerificationEmailAsync(user.Email, verificationCode, user.Name);

            return true;
        }
    }
}
