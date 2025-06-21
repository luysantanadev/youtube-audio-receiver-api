using Microsoft.Extensions.Logging;
using Raven.Client.Documents;

namespace AudioReceiverApi;

public sealed class AudioRepository : IAudioRepository
{
    private readonly IDocumentStore _store;
    private readonly ILogger<AudioRepository> _logger;

    public AudioRepository(IDocumentStore store, ILogger<AudioRepository> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task SaveAsync(Audio audio)
    {
        try
        {
            _logger.LogInformation("Iniciando persistência do áudio do usuário {UserId}.", audio.UserId);

            using var session = _store.OpenAsyncSession();
            await session.StoreAsync(audio);
            await session.SaveChangesAsync();

            _logger.LogInformation("Áudio persistido com sucesso. ID do MinIO: {MinioPath}, Timestamp: {CreatedAt}",
                audio.MinioFilePath, audio.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar o áudio do usuário {UserId} no RavenDB.", audio.UserId);
            throw;
        }
    }
}