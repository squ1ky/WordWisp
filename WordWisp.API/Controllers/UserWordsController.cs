using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordWisp.API.Models.DTOs.Words;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/dictionaries/{dictionaryId}/words")]
    public class UserWordsController : ControllerBase
    {
        private readonly IWordService _wordService;
        private readonly IDictionaryService _dictionaryService;

        public UserWordsController(IWordService wordService, IDictionaryService dictionaryService)
        {
            _wordService = wordService;
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
        public async Task<ActionResult<List<WordDto>>> GetWords(int userId, int dictionaryId, [FromQuery] string? search = null)
        {
            bool hasAccess;

            if (IsOwner(userId))
            {
                hasAccess = await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }
            else
            {
                var dictionary = await _dictionaryService.GetPublicDictionaryByIdAsync(dictionaryId);
                hasAccess = dictionary != null && await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }

            if (!hasAccess)
                return StatusCode(403, new { message = "Нет доступа к словарю" });

            List<WordDto> words;
            if (string.IsNullOrWhiteSpace(search))
            {
                words = await _wordService.GetWordsByDictionaryIdAsync(dictionaryId, userId);
            }
            else
            {
                words = await _wordService.SearchWordsAsync(dictionaryId, search, userId);
            }

            return Ok(words);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<WordDto>> CreateWord(int userId, int dictionaryId, [FromBody] CreateWordRequest request)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно добавлять слова только в свои словари" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var word = await _wordService.CreateWordAsync(request, dictionaryId, userId);
                return CreatedAtAction("GetWord", new { userId, dictionaryId, wordId = word.Id }, word);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Нет доступа к словарю");
            }
        }

        [HttpGet("{wordId}")]
        [AllowAnonymous]
        public async Task<ActionResult<WordDto>> GetWord(int userId, int dictionaryId, int wordId)
        {
            bool hasAccess;

            if (IsOwner(userId))
            {
                hasAccess = await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }
            else
            {
                var dictionary = await _dictionaryService.GetPublicDictionaryByIdAsync(dictionaryId);
                hasAccess = dictionary != null && await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }

            if (!hasAccess)
                return StatusCode(403, new { message = "Нет доступа к словарю" });

            var word = await _wordService.GetWordByIdAsync(wordId, userId);
            if (word == null)
                return StatusCode(404, new { message = "Слово не найдено" });

            return Ok(word);
        }

        [HttpPut("{wordId}")]
        [Authorize]
        public async Task<ActionResult<WordDto>> UpdateWord(int userId, int dictionaryId, int wordId, [FromBody] UpdateWordRequest request)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно редактировать только свои слова" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var word = await _wordService.UpdateWordAsync(wordId, request, userId);

            if (word == null)
                return StatusCode(404, new { message = "Слово не найдено" });

            return Ok(word);
        }

        [HttpDelete("{wordId}")]
        [Authorize]
        public async Task<IActionResult> DeleteWord(int userId, int dictionaryId, int wordId)
        {
            if (!IsOwner(userId))
                return StatusCode(403, new { message = "Можно удалять только свои слова" });

            var result = await _wordService.DeleteWordAsync(wordId, userId);

            if (!result)
                return StatusCode(404, new { message = "Слово не найдено" });

            return NoContent();
        }
    }
}
