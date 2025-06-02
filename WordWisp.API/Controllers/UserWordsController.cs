using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Models.DTOs.Words;
using WordWisp.API.Services.Interfaces;
using WordWisp.API.Constants;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/dictionaries/{dictionaryId}/words")]
    public class UserWordsController : ControllerBase
    {
        private readonly IWordService _wordService;
        private readonly IDictionaryService _dictionaryService;
        private readonly IUserContextService _userContext;

        public UserWordsController(IWordService wordService,
                                   IDictionaryService dictionaryService,
                                   IUserContextService userContext)
        {
            _wordService = wordService;
            _dictionaryService = dictionaryService;
            _userContext = userContext;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<WordDto>> CreateWord(int userId, int dictionaryId, [FromBody] CreateWordRequest request)
        {
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanAddWordsOnlyToOwnDictionaries });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var word = await _wordService.CreateWordAsync(request, dictionaryId, userId);
                return CreatedAtAction(nameof(GetWord), new { userId, dictionaryId, wordId = word.Id }, word);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = ErrorMessages.DictionaryAccessDenied });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<WordDto>>> GetWords(int userId, int dictionaryId, [FromQuery] string? search = null)
        {
            var hasAccess = await HasDictionaryAccess(userId, dictionaryId);

            if (!hasAccess)
                return StatusCode(403, new { message = ErrorMessages.DictionaryAccessDenied });

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

        [HttpGet("{wordId}")]
        [AllowAnonymous]
        public async Task<ActionResult<WordDto>> GetWord(int userId, int dictionaryId, int wordId)
        {
            var hasAccess = await HasDictionaryAccess(userId, dictionaryId);

            if (!hasAccess)
                return StatusCode(403, new { message = ErrorMessages.DictionaryAccessDenied });

            var word = await _wordService.GetWordByIdAsync(wordId, userId);
            if (word == null)
                return StatusCode(404, new { message = ErrorMessages.WordNotFound });

            return Ok(word);
        }

        [HttpPut("{wordId}")]
        [Authorize]
        public async Task<ActionResult<WordDto>> UpdateWord(int userId, int dictionaryId, int wordId, [FromBody] UpdateWordRequest request)
        {
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanEditOnlyOwnWords });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var word = await _wordService.UpdateWordAsync(wordId, request, userId);

            if (word == null)
                return StatusCode(404, new { message = ErrorMessages.WordNotFound });

            return Ok(word);
        }

        [HttpDelete("{wordId}")]
        [Authorize]
        public async Task<IActionResult> DeleteWord(int userId, int dictionaryId, int wordId)
        {
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanDeleteOnlyOwnWords });

            var result = await _wordService.DeleteWordAsync(wordId, userId);

            if (!result)
                return StatusCode(404, new { message = ErrorMessages.WordNotFound });

            return NoContent();
        }

        private async Task<bool> HasDictionaryAccess(int userId, int dictionaryId)
        {
            if (_userContext.IsOwner(userId))
            {
                return await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }
            else
            {
                var dictionary = await _dictionaryService.GetPublicDictionaryByIdAsync(dictionaryId);
                return dictionary != null && await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);
            }
        }
    }
}
