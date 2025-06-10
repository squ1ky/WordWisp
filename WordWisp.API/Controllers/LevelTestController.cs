using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Constants;
using WordWisp.API.Models.Entities.LevelTest;
using WordWisp.API.Models.DTOs.LevelTest;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/level-test")]
    [Authorize]
    public class LevelTestController : ControllerBase
    {
        private readonly ILevelTestService _levelTestService;
        private readonly IUserContextService _userContextService;

        public LevelTestController(ILevelTestService levelTestService, IUserContextService userContextService)
        {
            _levelTestService = levelTestService;
            _userContextService = userContextService;
        }

        [HttpGet("eligibility")]
        public async Task<IActionResult> CheckEligibility()
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var canStart = await _levelTestService.CanStartNewTestAsync(userId.Value);

            if (!canStart)
            {
                return StatusCode(429, new { message = ErrorMessages.RetestingNotAvailable });
            }

            return Ok(new { eligible = true });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTest()
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var activeTest = await _levelTestService.GetActiveTestAsync(userId.Value);

            if (activeTest == null)
            {
                return StatusCode(404, new { message = ErrorMessages.NotFound });
            }

            return Ok(activeTest);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTest()
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var session = await _levelTestService.StartTestAsync(userId.Value);

            if (session == null)
            {
                return StatusCode(400, new { message = ErrorMessages.UnableStartTest });
            }

            return CreatedAtAction(nameof(GetActiveTest), new { id = session.Id }, session);
        }

        [HttpGet("{testId}/questions/next")]
        public async Task<IActionResult> GetNextQuestion(int testId, [FromQuery] string section)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            if (!Enum.TryParse<QuestionSection>(section, true, out var questionSection))
            {
                return StatusCode(400, new { message = ErrorMessages.WrongSection });
            }

            var question = await _levelTestService.GetNextQuestionAsync(testId, questionSection, userId.Value);

            if (question == null)
            {
                return Ok(new { completed = true, section = section });
            }

            return Ok(question);
        }

        [HttpPost("{testId}/answers")]
        public async Task<IActionResult> SubmitAnswer(int testId, [FromBody] SubmitAnswerRequest request)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            if (!ModelState.IsValid)
            {
                return StatusCode(400, new { message = ErrorMessages.InvalidRequestData });
            }

            var success = await _levelTestService.SubmitAnswerAsync(
                testId,
                request.QuestionId,
                request.SelectedAnswer,
                userId.Value
            );

            if (!success)
            {
                return StatusCode(400, new { message = ErrorMessages.FailedSaveAnswer });
            }

            return Ok(new { success = true });
        }

        [HttpPost("{testId}/complete")]
        public async Task<IActionResult> CompleteTest(int testId)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var result = await _levelTestService.CompleteTestAsync(testId, userId.Value);

            if (result == null)
            {
                return StatusCode(400, new { message = ErrorMessages.UnableCompleteTest });
            }

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetTestHistory()
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var history = await _levelTestService.GetTestHistoryAsync(userId.Value);

            return Ok(history);
        }

        [HttpPost("{testId}/send-certificate")]
        public async Task<IActionResult> SendCertificate(int testId)
        {
            var userId = _userContextService.GetCurrentUserId();
            if (userId == null) return StatusCode(401, new { message = ErrorMessages.AccessDenied });

            var success = await _levelTestService.SendCertificateAsync(testId, userId.Value);

            if (!success)
            {
                return StatusCode(400, new { message = "Не удалось отправить сертификат" });
            }

            return Ok(new { message = "Сертификат отправлен на вашу почту" });
        }
    }
}
