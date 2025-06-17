using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Constants;
using WordWisp.API.Models.Enums;
using WordWisp.API.Models.Requests.Materials;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api/materials")]
    [Authorize]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly IUserContextService _userContext;
        private readonly ILogger<MaterialController> _logger;

        public MaterialController(IMaterialService materialService, IUserContextService userContext, ILogger<MaterialController> logger)
        {
            _materialService = materialService;
            _userContext = userContext;
            _logger = logger;
        }

        // Получить материал по ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaterial(int id)
        {
            try
            {
                var userId = _userContext.GetCurrentUserId();
                var material = await _materialService.GetMaterialByIdAsync(id, userId);
                
                if (material == null)
                    return NotFound(new { message = ErrorMessages.MaterialNotFound });

                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting material {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Получить материалы по топику
        [HttpGet("topic/{topicId}")]
        public async Task<IActionResult> GetMaterialsByTopic(int topicId)
        {
            try
            {
                var userId = _userContext.GetCurrentUserId();
                var materials = await _materialService.GetMaterialsByTopicIdAsync(topicId, userId);
                
                return Ok(materials);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting materials for topic {topicId}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Создать текстовый или видео материал
        [HttpPost]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> CreateMaterial([FromBody] CreateMaterialRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userContext.GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });

                // Проверяем, что для текста и видео не требуется файл
                if (request.MaterialType != MaterialType.Text && request.MaterialType != MaterialType.Video)
                {
                    return BadRequest(new { message = "Для изображений и аудио используйте endpoint с загрузкой файла" });
                }

                var material = await _materialService.CreateMaterialAsync(request, userId.Value);
                return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating material: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Создать материал с файлом (изображение или аудио)
        [HttpPost("upload")]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> CreateMaterialWithFile([FromForm] CreateMaterialWithFileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userContext.GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });

                // Проверяем, что тип материала требует файл
                if (request.MaterialType != MaterialType.Image && request.MaterialType != MaterialType.Audio)
                {
                    return BadRequest(new { message = "Этот endpoint предназначен только для изображений и аудио" });
                }

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { message = "Файл обязателен" });
                }

                var createRequest = new CreateMaterialRequest
                {
                    Title = request.Title,
                    Description = request.Description,
                    MaterialType = request.MaterialType,
                    IsPublic = request.IsPublic,
                    Order = request.Order,
                    TopicId = request.TopicId
                };

                var material = await _materialService.CreateMaterialWithFileAsync(createRequest, request.File, userId.Value);
                return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating material with file: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Обновить материал
        [HttpPut("{id}")]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] UpdateMaterialRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userContext.GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });

                var material = await _materialService.UpdateMaterialAsync(id, request, userId.Value);
                return Ok(material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating material {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Обновить материал с файлом
        [HttpPut("{id}/upload")]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> UpdateMaterialWithFile(int id, [FromForm] UpdateMaterialWithFileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userContext.GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new { message = "Файл обязателен" });
                }

                var updateRequest = new UpdateMaterialRequest
                {
                    Title = request.Title,
                    Description = request.Description,
                    Content = request.Content,
                    ExternalUrl = request.ExternalUrl,
                    IsPublic = request.IsPublic,
                    Order = request.Order
                };

                var material = await _materialService.UpdateMaterialWithFileAsync(id, updateRequest, request.File, userId.Value);
                return Ok(material);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating material {id} with file: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Удалить материал
        [HttpDelete("{id}")]
        [Authorize(Roles = "2,Teacher")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                var userId = _userContext.GetCurrentUserId();
                if (!userId.HasValue)
                    return StatusCode(403, new { message = ErrorMessages.AccessDenied });

                await _materialService.DeleteMaterialAsync(id, userId.Value);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting material {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        // Скачать файл материала
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var userId = _userContext.GetCurrentUserId();
                var material = await _materialService.GetMaterialByIdAsync(id, userId);
                
                if (material == null)
                    return NotFound(new { message = ErrorMessages.MaterialNotFound });

                if (string.IsNullOrEmpty(material.FilePath))
                    return BadRequest(new { message = "У материала нет файла для скачивания" });

                var fileService = HttpContext.RequestServices.GetRequiredService<IFileService>();
                var fileStream = await fileService.GetFileStreamAsync(material.FilePath);
                
                if (fileStream == null)
                    return NotFound(new { message = "Файл не найден" });

                return File(fileStream, material.MimeType ?? "application/octet-stream", material.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading file for material {id}: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }
    }
}
