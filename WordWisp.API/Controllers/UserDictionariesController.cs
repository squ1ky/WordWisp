using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Models.DTOs.Dictionaries;
using WordWisp.API.Services.Interfaces;
using WordWisp.API.Constants;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/dictionaries")]
    public class UserDictionariesController : ControllerBase
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly IUserContextService _userContext;

        public UserDictionariesController(IDictionaryService dictionaryService,
                                          IUserContextService userContext)
        {
            _dictionaryService = dictionaryService;
            _userContext = userContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<DictionaryDto>>> GetUserDictionaries(int userId)
        {
            if (_userContext.IsOwner(userId))
            {
                var allDictionaries = await _dictionaryService.GetUserDictionariesAsync(userId);
                return Ok(allDictionaries);
            }

            var publicDictionaries = await _dictionaryService.GetUserPublicDictionariesAsync(userId);

            return Ok(publicDictionaries);
        }

        [HttpGet("{dictionaryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<DictionaryDetailDto>> GetUserDictionary(int userId, int dictionaryId)
        {
            if (_userContext.IsOwner(userId))
            {
                var dictionary = await _dictionaryService.GetDictionaryByIdAsync(dictionaryId, userId);

                if (dictionary == null)
                {
                    return StatusCode(404, new { message = ErrorMessages.DictionaryNotFound });
                }

                return Ok(dictionary);
            }

            var isUsersDictionary = await _dictionaryService.CheckDictionaryOwnershipAsync(dictionaryId, userId);

            if (!isUsersDictionary)
            {
                return StatusCode(404, new { message = ErrorMessages.DictionaryNotBelongsToUser });
            }

            var publicDictionary = await _dictionaryService.GetPublicDictionaryByIdAsync(dictionaryId);

            if (publicDictionary == null)
            {
                return StatusCode(403, new { message = ErrorMessages.DictionaryAccessDenied });
            }

            return Ok(publicDictionary);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DictionaryDto>> CreateDictionary(int userId, [FromBody] CreateDictionaryRequest request)
        {
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanCreateOnlyOwnDictionaries });

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
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanEditOnlyOwnDictionaries });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dictionary = await _dictionaryService.UpdateDictionaryAsync(dictionaryId, request, userId);

            if (dictionary == null)
            {
                return StatusCode(404, new { message = ErrorMessages.DictionaryNotFound });
            }

            return Ok(dictionary);
        }

        [HttpDelete("{dictionaryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteDictionary(int userId, int dictionaryId)
        {
            if (!_userContext.IsOwner(userId))
                return StatusCode(403, new { message = ErrorMessages.CanDeleteOnlyOwnDictionaries });

            var result = await _dictionaryService.DeleteDictionaryAsync(dictionaryId, userId);

            if (!result)
            {
                return StatusCode(404, new { message = ErrorMessages.DictionaryNotFound });
            }

            return NoContent();
        }
    }
}
