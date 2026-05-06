using Microsoft.AspNetCore.Http;

namespace nsia.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileService> _logger;

        private string RootPath => _config["UploadSettings:RootPath"]
            ?? throw new InvalidOperationException("UploadSettings:RootPath not configured.");

        private string BaseUrl => _config["UploadSettings:BaseUrl"]
            ?? throw new InvalidOperationException("UploadSettings:BaseUrl not configured.");

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
        private static readonly string[] AllowedExtensions =
            { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg" };

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

            // Build folder: /var/www/npi/uploads/applications/{applicationId}/
            var relativeFolder = Path.Combine("applications", applicationId.ToString());
            var absoluteFolder = Path.Combine(RootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            // Sanitise original filename
            var safeName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_")
                .Replace("..", "")
                .Replace("/", "")
                .Replace("\\", "");

            safeName = safeName.Length > 60 ? safeName[..60] : safeName;

            var uniqueName = $"{safeName}_{Guid.NewGuid():N}{ext}";

            // Absolute path on disk
            var absolutePath = Path.Combine(absoluteFolder, uniqueName);

            // Relative stored path — used to reconstruct the public URL
            // e.g. applications/550e8400-e29b.../myfile_abc123.pdf
            var storedPath = Path.Combine(relativeFolder, uniqueName)
                                   .Replace("\\", "/"); // normalise for all OS

            // Public URL — e.g. https://nsia.com/uploads/applications/550e.../myfile.pdf
            var publicUrl = $"{BaseUrl.TrimEnd('/')}/{storedPath}";

            using var stream = new FileStream(absolutePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation(
                "Saved file {FileName} to {AbsolutePath}. Public URL: {PublicUrl}",
                file.FileName, absolutePath, publicUrl);

            return (publicUrl, file.FileName);
        }

        public void DeleteDocument(string storedPath)
        {
            try
            {
                var absolutePath = Path.Combine(RootPath, storedPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(absolutePath))
                    File.Delete(absolutePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file at {StoredPath}", storedPath);
            }
        }
    }
}