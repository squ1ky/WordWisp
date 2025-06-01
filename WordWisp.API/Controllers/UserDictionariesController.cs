using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordWisp.API.Models.DTOs.Dictionaries;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/dictionaries")]
    public class UserDictionariesController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;

        public UserDictionariesController(IDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        private int? GetCurrentUserId()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private bool IsOwner(int userId)
        {
            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && currentUserId.Value == userId;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<DictionaryDto>>> GetUserDictionaries(int userId)
        {
            if (IsOwner(userId))
            {
                var allDictionaries = await _dictionaryService.GetUserDictionariesAsync(userId);
                return Ok(allDictionaries);
            }
            else
            {
                var publicDictionaries = await _dictionaryService.GetUserPublicDictionariesAsync(userId);
                return Ok(publicDictionaries);
            }
        }

        [HttpGet("{dictionaryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<DictionaryDetailDto>> GetUserDictionary(int userId, int dictionaryId)
        {
            if (IsOwner(userId))
            {
                var dictionary = await _dictionaryService.GetDictionaryByIdAsync(dictionaryId, userId);

                if (dictionary == null)
                    return StatusCode(404, new
                    {
                        message = "Словарь не найден"
                    });

                return Ok(dictionary);
            }
            else
            {
                var isUsersDictionary = await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
                if (!isUsersDictionary)
                    return StatusCode(404, new
                    {
                        message = "Словарь не принадлежит указанному пользователю"
                    });

                var publicDictionary = await _dictionaryService.GetPublicDictionaryByIdAsync(dictionaryId);
                if (publicDictionary == null)
                {
                    return StatusCode(403, new
                    {
                        message = "У вас нет доступа к этому словарю"
                    });
                }

                return Ok(publicDictionary);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DictionaryDto>> CreateDictionary(int userId, [FromBody] CreateDictionaryRequest request)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно создавать словари только для себя" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dictionary = await _dictionaryService.CreateDictionaryAsync(request, userId);

            return CreatedAtAction(nameof(GetUserDictionary),
                new { userId = userId, dictionaryId = dictionary.Id }, dictionary);
        }

        [HttpPut("{dictionaryId}")]
        [Authorize]
        public async Task<ActionResult<DictionaryDto>> UpdateDictionary(int userId, int dictionaryId, [FromBody] UpdateDictionaryRequest request)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно редактировать только свои словари" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dictionary = await _dictionaryService.UpdateDictionaryAsync(dictionaryId, request, userId);

            if (dictionary == null)
                return StatusCode(404, new
                {
                    message = "Словарь не найден"
                }); ;

            return Ok(dictionary);
        }

        [HttpDelete("{dictionaryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteDictionary(int userId, int dictionaryId)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно удалять только свои словари" });

            var result = await _dictionaryService.DeleteDictionaryAsync(dictionaryId, userId);

            if (!result)
                return StatusCode(404, new
                {
                    message = "Словарь не найден"
                });

            return NoContent();
        }
    }
}
