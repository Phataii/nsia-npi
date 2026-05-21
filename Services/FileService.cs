using Microsoft.AspNetCore.Http;

namespace nsia.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileService> _logger;

        private string RootPath => _config["UploadSettings:RootPath"]
            ?? throw new InvalidOperationException("UploadSettings:RootPath not configured.");

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        private static readonly string[] AllowedExtensions =
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg"
        };

        public FileService(IConfiguration config, ILogger<FileService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<(string storedPath, string fileName)> SaveDocumentAsync(
            IFormFile file,
            Guid applicationId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("File exceeds the 10MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException($"File type '{ext}' is not allowed.");

            var originalFileName = Path.GetFileName(file.FileName);

            var relativeFolder = Path.Combine("applications", applicationId.ToString());
            var absoluteFolder = Path.Combine(RootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var safeName = Path.GetFileNameWithoutExtension(originalFileName)
                .Replace(" ", "_")
                .Replace("..", "")
                .Replace("/", "")
                .Replace("\\", "");

            safeName = string.IsNullOrWhiteSpace(safeName) ? "document" : safeName;
            safeName = safeName.Length > 60 ? safeName[..60] : safeName;

            var uniqueName = $"{safeName}_{Guid.NewGuid():N}{ext}";
            var absolutePath = Path.Combine(absoluteFolder, uniqueName);

            var storedPath = Path.Combine(relativeFolder, uniqueName)
                .Replace("\\", "/");

            await using var stream = new FileStream(absolutePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation(
                "Saved file {OriginalFileName} to {AbsolutePath} (stored path: {StoredPath})",
                originalFileName, absolutePath, storedPath);

            return (storedPath, originalFileName);
        }

        public void DeleteFile(string storedPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedPath))
                    return;

                var normalisedPath = storedPath.Replace('/', Path.DirectorySeparatorChar);

                var fullPath = Path.Combine(RootPath, normalisedPath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Deleted file at {FullPath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file at stored path {StoredPath}", storedPath);
            }
        }

        public string GetPublicUrl(string storedPath)
        {
            var baseUrl = _config["UploadSettings:BaseUrl"]
                ?? throw new InvalidOperationException("UploadSettings:BaseUrl not configured.");

            return $"{baseUrl.TrimEnd('/')}/{storedPath.TrimStart('/')}";
        }
    }
}
