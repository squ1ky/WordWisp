namespace WordWisp.API.Services.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult> SaveFileAsync(IFormFile file, string directory);
        Task<bool> DeleteFileAsync(string filePath);
        Task<FileStream?> GetFileStreamAsync(string filePath);
        string GetFileUrl(string filePath);
        bool IsValidFileType(IFormFile file, string[] allowedTypes);
        bool IsValidFileSize(IFormFile file, long maxSizeBytes);
        string GenerateUniqueFileName(string originalFileName);
    }

    public class FileUploadResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
