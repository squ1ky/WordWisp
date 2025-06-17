using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<FileService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<FileUploadResult> SaveFileAsync(IFormFile file, string directory)
        {
            try
            {
                // ИСПРАВЛЕНИЕ: Проверяем, что WebRootPath не null
                var webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    // Если WebRootPath не установлен, создаем папку wwwroot
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                    if (!Directory.Exists(webRootPath))
                    {
                        Directory.CreateDirectory(webRootPath);
                    }
                }

                // Создаем директорию для загрузок
                var uploadsPath = Path.Combine(webRootPath, "uploads", directory);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Генерируем уникальное имя файла
                var uniqueFileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Создаем относительный путь для хранения в БД
                var relativePath = Path.Combine("uploads", directory, uniqueFileName).Replace("\\", "/");

                return new FileUploadResult
                {
                    FilePath = relativePath,
                    FileName = uniqueFileName,
                    OriginalFileName = file.FileName,
                    FileSize = file.Length,
                    MimeType = file.ContentType,
                    FileUrl = GetFileUrl(relativePath)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving file {file.FileName}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file {filePath}: {ex.Message}");
                return false;
            }
        }

        public async Task<FileStream?> GetFileStreamAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    return await Task.FromResult(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting file stream {filePath}: {ex.Message}");
                return null;
            }
        }

        public string GetFileUrl(string filePath)
        {
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5118";
            return $"{baseUrl}/{filePath.Replace("\\", "/")}";
        }

        public bool IsValidFileType(IFormFile file, string[] allowedTypes)
        {
            return allowedTypes.Contains(file.ContentType.ToLower());
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeBytes)
        {
            return file.Length <= maxSizeBytes;
        }

        public string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var guid = Guid.NewGuid().ToString("N")[..8];
            
            return $"{fileNameWithoutExtension}_{timestamp}_{guid}{extension}";
        }
    }
}
