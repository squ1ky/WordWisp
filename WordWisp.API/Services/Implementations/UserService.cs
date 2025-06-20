﻿using WordWisp.API.Constants;
using WordWisp.API.Data.Repositories.Interfaces;
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
        private readonly IDictionaryRepository _dictionaryRepository; 
        private readonly ILogger<UserService> _logger;
        private readonly IWordRepository _wordRepository;
        private readonly ILevelTestRepository _levelTestRepository;
        private readonly ITopicRepository _topicRepository;
        public UserService(IUserRepository userRepository, IEmailService emailService, ILogger<UserService> logger, IDictionaryRepository dictionaryRepository, IWordRepository wordRepository, ILevelTestRepository levelTestRepository, ITopicRepository topicRepository)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _logger = logger;
            _dictionaryRepository = dictionaryRepository;
            _wordRepository = wordRepository;
            _levelTestRepository = levelTestRepository;
            _topicRepository = topicRepository;
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
                throw new ArgumentException(ErrorMessages.UserNotFound);

            _logger.LogInformation($"Current user data: Username={user.Username}, Name={user.Name}, Surname={user.Surname}, Email={user.Email}, Role={user.Role}");

            // Проверяем уникальность username только если он изменился
            if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameExists = await _userRepository.ExistsByUsernameAsync(request.Username, id);
                if (usernameExists)
                    throw new ArgumentException(ErrorMessages.UsernameAlreadyExists);
            }

            bool emailChanged = !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase);
            if (emailChanged)
            {
                var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, id);
                if (emailExists)
                    throw new ArgumentException(ErrorMessages.EmailAlreadyExists);
            }

            user.Username = request.Username;
            user.Name = request.Name;
            user.Surname = request.Surname;
            user.Role = request.Role;

            // Если email изменился, сбрасываем верификацию и отправляем код
            if (emailChanged)
            {
                user.Email = request.Email;
                user.IsEmailVerified = false;

                var verificationCode = new Random().Next(100000, 999999).ToString();
                user.EmailVerificationCode = verificationCode;
                user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

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

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new ArgumentException(ErrorMessages.CurrentPasswordIncorrect);

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
                throw new ArgumentException(ErrorMessages.EmailAlreadyExists);

            var verificationCode = new Random().Next(100000, 999999).ToString();
            user.EmailVerificationCode = verificationCode;
            user.EmailVerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);

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

                int dictionariesCount = 0;
                int wordsCount = 0;

                var lastTestLevel = await _levelTestRepository.GetLastTestLevelAsync(id);
                var lastTestDate = await _levelTestRepository.GetLastTestDateAsync(id);
                var lastTestId = await _levelTestRepository.GetLastTestIdAsync(id);
                var testHistory = await _levelTestRepository.GetUserTestHistoryAsync(id);
                var testCount = testHistory.Count;

                var topicCount = await _topicRepository.GetTopicsCountByTeacherAsync(id);
                var totalMaterialsCount = await _topicRepository.GetTotalMaterialsCountByTeacherAsync(id);
                try
                {
                    if (_dictionaryRepository != null)
                    {
                        dictionariesCount = await _dictionaryRepository.GetDictionariesCountByUserIdAsync(id);
                        wordsCount = await _wordRepository.GetWordsCountByUserIdAsync(id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error getting dictionaries count: {ex.Message}");
                    dictionariesCount = 0; 
                }

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
                        wordsCount = wordsCount,
                        lastTestLevel = lastTestLevel,
                        lastTestDate = lastTestDate,
                        lastTestId = lastTestId,
                        testCount = testCount
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
                        wordsCount = wordsCount,
                        createdMaterialsCount = topicCount,
                        totalMaterialsCount = totalMaterialsCount
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
