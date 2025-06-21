using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace AudioReceiverApi;

public interface IMinioUploader
{
    Task<(string Path, string Etag)> SaveAsync(IFormFile file, string userId);
}

public sealed class MinioUploader : IMinioUploader
{
    private readonly IMinioClient _minio;
    private readonly IConfiguration _config;
    private readonly ILogger<MinioUploader> _logger;

    public MinioUploader(IMinioClient minio, IConfiguration config, ILogger<MinioUploader> logger)
    {
        _minio = minio;
        _config = config;
        _logger = logger;
    }

    public async Task<(string Path, string Etag)> SaveAsync(IFormFile file, string userId)
    {
        var bucket = _config["MINIO_BUCKET_NAME"]!;
        var sourceName = Regex.Replace(file.FileName, "\\s", "-").ToLower();
        var fileName = $"{userId}/{Guid.NewGuid()}-{sourceName}";

        try
        {
            _logger.LogInformation("Iniciando upload do arquivo {FileName} para o bucket {Bucket}, usuário {UserId}", file.FileName, bucket, userId);

            var bucketExists = await _minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
            if (!bucketExists)
            {
                _logger.LogWarning("Bucket {Bucket} não encontrado. Criando novo bucket.", bucket);
                await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
            }

            var newBlobObjetc = new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName)
                .WithContentType(file.ContentType)
                .WithStreamData(file.OpenReadStream())
                .WithObjectSize(file.Length);

            var response = await _minio.PutObjectAsync(newBlobObjetc);

            _logger.LogInformation("Upload concluído com sucesso. Objeto: {NomeObjeto}, ETag: {ETag}", fileName, response.Etag);

            return (fileName, response.Etag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar arquivo {FileName} para o MinIO. Usuário: {UserId}", file.FileName, userId);
            throw;
        }
    }
}