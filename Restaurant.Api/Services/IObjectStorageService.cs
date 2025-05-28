namespace Restaurant.Api.Services;

public interface IObjectStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName);
}