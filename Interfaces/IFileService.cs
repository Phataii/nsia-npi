namespace nsia.Services
{
    public interface IFileService
    {
        Task<(string storedPath, string fileName)> SaveDocumentAsync(
            IFormFile file,
            Guid applicationId);

        // void DeleteDocument(string storedPath);
        void DeleteFile(string storedPath);
    }
}