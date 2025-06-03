using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Models.Requests.Users;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserContextService _userContext;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, IUserContextService userContext, ILogger<UserController> logger)
        {
            _userService = userService;
            _userContext = userContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            _logger.LogInformation($"GET request for user {id}");

            if (!_userContext.IsOwner(id))
                return StatusCode(403, new { message = "Доступ запрещен" });

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден" });

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            _logger.LogInformation($"PUT request for user {id} with data: Username={request.Username}, Name={request.Name}, Surname={request.Surname}, Email={request.Email}, Role={request.Role}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid model state for user {id}: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            if (!_userContext.IsOwner(id))
            {
                _logger.LogWarning($"Access denied for user {id}");
                return StatusCode(403, new { message = "Доступ запрещен" });
            }

            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                _logger.LogInformation($"Successfully updated user {id}");
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Update error for user {id}: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error updating user {id}: {ex.Message}");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            _logger.LogInformation($"Change password request for user {id}");

            if (!_userContext.IsOwner(id))
                return StatusCode(403, new { message = "Доступ запрещен" });

            try
            {
                var result = await _userService.ChangePasswordAsync(id, request);
                if (result)
                    return Ok(new { message = "Пароль успешно изменен" });

                return NotFound(new { message = "Пользователь не найден" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetUserStats(int id)
        {
            _logger.LogInformation($"GET stats request for user {id}");

            if (!_userContext.IsOwner(id))
                return StatusCode(403, new { message = "Доступ запрещен" });

            try
            {
                var stats = await _userService.GetUserStatsAsync(id);
                if (stats == null)
                    return NotFound(new { message = "Пользователь не найден" });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user stats {id}: {ex.Message}");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

    }
}
