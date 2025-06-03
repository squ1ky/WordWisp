using WordWisp.API.Models.DTOs.Users;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Users;
using WordWisp.API.Repositories.Implementations;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IDictionaryRepository _dictionaryRepository; // Добавили
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IEmailService emailService, ILogger<UserService> logger, IDictionaryRepository dictionaryRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
            _dictionaryRepository = dictionaryRepository;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsEmailVerified = user.IsEmailVerified
            };
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            _logger.LogInformation($"Updating user {id}: Username={request.Username}, Name={request.Name}, Surname={request.Surname}, Email={request.Email}, Role={request.Role}");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException("Пользователь не найден");

            _logger.LogInformation($"Current user data: Username={user.Username}, Name={user.Name}, Surname={user.Surname}, Email={user.Email}, Role={user.Role}");

            // Проверяем уникальность username только если он изменился
            if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameExists = await _userRepository.ExistsByUsernameAsync(request.Username, id);
                if (usernameExists)
                    throw new ArgumentException("Пользователь с таким именем уже существует");
            }

            // Проверяем уникальность email только если он изменился
            bool emailChanged = !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase);
            if (emailChanged)
            {
                var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, id);
                if (emailExists)
                    throw new ArgumentException("Пользователь с таким email уже существует");
            }

            // Обновляем данные
            user.Username = request.Username;
            user.Name = request.Name;
            user.Surname = request.Surname;
            user.Role = request.Role;

            // Если email изменился, сбрасываем верификацию и отправляем код
            if (emailChanged)
            {
                user.Email = request.Email;
                user.IsEmailVerified = false;

                // Генерируем код подтверждения
                var verificationCode = new Random().Next(100000, 999999).ToString();
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

                // Отправляем email с кодом
                try
                {
                    await _emailService.SendVerificationEmailAsync(user.Email, verificationCode, user.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Email sending error: {ex.Message}");
                }
            }

            _logger.LogInformation($"Saving user {id} with new data: Username={user.Username}, Name={user.Name}, Surname={user.Surname}, Email={user.Email}, Role={user.Role}");

            var updatedUser = await _userRepository.UpdateAsync(user);

            _logger.LogInformation($"User {id} updated successfully");

            return new UserDto
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                Name = updatedUser.Name,
                Surname = updatedUser.Surname,
                Email = updatedUser.Email,
                Role = updatedUser.Role,
                CreatedAt = updatedUser.CreatedAt,
                IsEmailVerified = updatedUser.IsEmailVerified
            };
        }

        public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            // Проверяем текущий пароль
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new ArgumentException("Неверный текущий пароль");

            // Хешируем новый пароль
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> ChangeEmailAsync(int id, string newEmail)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return false;

            var emailExists = await _userRepository.ExistsByEmailAsync(newEmail, id);
            if (emailExists)
                throw new ArgumentException("Пользователь с таким email уже существует");

            // Генерируем код подтверждения
            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            // Отправляем email с кодом на новый адрес
            await _emailService.SendVerificationEmailAsync(newEmail, verificationCode, user.Name);

            return true;
        }

        public async Task<object?> GetUserStatsAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with id {id} not found");
                    return null;
                }

                // Безопасно получаем количество словарей
                int dictionariesCount = 0;
                try
                {
                    // Если DictionaryRepository не внедрен, используем заглушку
                    if (_dictionaryRepository != null)
                    {
                        dictionariesCount = await _dictionaryRepository.GetDictionariesCountByUserIdAsync(id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error getting dictionaries count: {ex.Message}");
                    dictionariesCount = 0; // Заглушка
                }

                // Создаем заглушки в зависимости от роли
                if (user.Role == UserRole.Student)
                {
                    return new
                    {
                        userId = user.Id,
                        username = user.Username,
                        name = user.Name,
                        surname = user.Surname,
                        email = user.Email,
                        role = user.Role,
                        isEmailVerified = user.IsEmailVerified,
                        createdAt = user.CreatedAt,
                        dictionariesCount = dictionariesCount,
                        englishLevel = (string?)null,
                        lastTestDate = (DateTime?)null,
                        studiedWordsCount = 0
                    };
                }
                else if (user.Role == UserRole.Teacher)
                {
                    return new
                    {
                        userId = user.Id,
                        username = user.Username,
                        name = user.Name,
                        surname = user.Surname,
                        email = user.Email,
                        role = user.Role,
                        isEmailVerified = user.IsEmailVerified,
                        createdAt = user.CreatedAt,
                        dictionariesCount = dictionariesCount,
                        createdMaterialsCount = 0,
                        studentsViewedCount = 0
                    };
                }

                return new
                {
                    userId = user.Id,
                    username = user.Username,
                    name = user.Name,
                    surname = user.Surname,
                    email = user.Email,
                    role = user.Role,
                    isEmailVerified = user.IsEmailVerified,
                    createdAt = user.CreatedAt,
                    dictionariesCount = dictionariesCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetUserStatsAsync: {ex.Message}");
                throw;
            }
        }


    }
}
