using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Constants;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Topics;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/topics")]
    [Authorize]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _topicService;
        private readonly IUserContextService _userContext;
        private readonly ILogger<TopicController> _logger;

        public TopicController(ITopicService topicService, IUserContextService userContext, ILogger<TopicController> logger)
        {
            _topicService = topicService;
            _userContext = userContext;
            _logger = logger;
        }

        // GET /api/topics/public - Получение всех публичных топиков (для студентов)
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicTopics([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"Getting public topics, page: {page}, pageSize: {pageSize}");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var topics = await _topicService.GetPublicTopicsAsync(page, pageSize);
                var totalCount = await _topicService.GetPublicTopicsCountAsync();

                var response = new
                {
                    topics = topics,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting public topics: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // GET /api/topics/public/{id} - Получение конкретного публичного топика (для студентов)
        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicTopic(int id)
        {
            try
            {
                _logger.LogInformation($"Getting public topic {id}");

                var topic = await _topicService.GetPublicTopicByIdAsync(id);
                
                if (topic == null)
                {
                    return NotFound(new { message = ErrorMessages.TopicNotFound });
                }

                return Ok(topic);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting public topic {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // GET /api/topics/teacher - Получение топиков преподавателя
        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherTopics([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId();
                if (teacherId == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                _logger.LogInformation($"Getting teacher topics for teacher {teacherId}, page: {page}, pageSize: {pageSize}");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var topics = await _topicService.GetTeacherTopicsAsync(teacherId.Value, page, pageSize);
                var totalCount = await _topicService.GetTeacherTopicsCountAsync(teacherId.Value);

                var response = new
                {
                    topics = topics,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teacher topics: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // GET /api/topics/teacher/{id} - Получение конкретного топика преподавателя
        [HttpGet("teacher/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetTeacherTopic(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId();
                if (teacherId == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                _logger.LogInformation($"Getting teacher topic {id} for teacher {teacherId}");

                var topic = await _topicService.GetTeacherTopicByIdAsync(id, teacherId.Value);
                
                if (topic == null)
                {
                    return NotFound(new { message = ErrorMessages.TopicNotFound });
                }

                return Ok(topic);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teacher topic {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // POST /api/topics - Создание топика (только для преподавателей)
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId();
                if (teacherId == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                _logger.LogInformation($"Creating topic '{request.Title}' for teacher {teacherId}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var topic = await _topicService.CreateTopicAsync(request, teacherId.Value);
                
                _logger.LogInformation($"Topic {topic.Id} created successfully");
                
                return CreatedAtAction(nameof(GetTeacherTopic), new { id = topic.Id }, topic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error creating topic: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error creating topic: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // PUT /api/topics/{id} - Обновление топика (только для преподавателей, только свои)
        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] UpdateTopicRequest request)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId();
                if (teacherId == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                _logger.LogInformation($"Updating topic {id} for teacher {teacherId}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var topic = await _topicService.UpdateTopicAsync(id, request, teacherId.Value);
                
                _logger.LogInformation($"Topic {id} updated successfully");
                
                return Ok(topic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error updating topic {id}: {ex.Message}");
                
                if (ex.Message == ErrorMessages.TopicNotFound)
                {
                    return NotFound(new { message = ex.Message });
                }
                
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error updating topic {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // DELETE /api/topics/{id} - Удаление топика (только для преподавателей, только свои)
        [HttpDelete("{id}")]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId();
                if (teacherId == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                _logger.LogInformation($"Deleting topic {id} for teacher {teacherId}");

                var result = await _topicService.DeleteTopicAsync(id, teacherId.Value);
                
                if (result)
                {
                    _logger.LogInformation($"Topic {id} deleted successfully");
                    return Ok(new { message = ErrorMessages.TopicDeletedSuccessfully });
                }
                else
                {
                    return NotFound(new { message = ErrorMessages.TopicNotFound });
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Error deleting topic {id}: {ex.Message}");
                
                if (ex.Message == ErrorMessages.TopicNotFound)
                {
                    return NotFound(new { message = ex.Message });
                }
                
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error deleting topic {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // GET /api/topics/{id}/access - Проверка доступа к топику (для любой роли)
        [HttpGet("{id}/access")]
        public async Task<IActionResult> CheckTopicAccess(int id)
        {
            try
            {
                var userId = _userContext.GetCurrentUserId();
                var userRole = _userContext.GetCurrentUserRole();

                int userIdInt = userId.GetValueOrDefault();

                if (userId == null || userRole == null)
                {
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });
                }

                var hasAccess = await _topicService.CanAccessTopicAsync(id, userIdInt, userRole);
                
                return Ok(new { hasAccess = hasAccess });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking topic access {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }
    }
}
