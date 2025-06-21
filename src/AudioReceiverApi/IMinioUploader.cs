namespace AudioReceiverApi;

public interface IMinioUploader
{
    Task<(string Path, string Etag)> SaveAsync(IFormFile file, string userId);
}