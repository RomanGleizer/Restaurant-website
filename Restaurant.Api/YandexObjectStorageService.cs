using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Restaurant.Api.Services;

public class YandexObjectStorageService : IObjectStorageService
{
    private readonly AmazonS3Client _s3Client;
    private readonly string _bucketName;

    public YandexObjectStorageService(string accessKey, string secretKey, string bucketName)
    {
        _bucketName = bucketName;

        var config = new AmazonS3Config
        {
            ServiceURL = "https://storage.yandexcloud.net",
            ForcePathStyle = true,
            AuthenticationRegion = "ru-central1"
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = fileStream,
            CannedACL = S3CannedACL.PublicRead
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"https://{_bucketName}.storage.yandexcloud.net/{fileName}";
    }

    public async Task<bool> BucketExistsAsync()
    {
        return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
    }
}
