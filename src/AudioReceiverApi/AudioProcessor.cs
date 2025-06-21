using FFMpegCore;
using FFMpegCore.Pipes;
using Microsoft.Extensions.Logging;

namespace AudioReceiverApi;

public sealed class AudioProcessor : IAudioProcessor
{
    private readonly ILogger<AudioProcessor> _logger;

    public AudioProcessor(ILogger<AudioProcessor> logger)
    {
        _logger = logger;
    }

    public async Task<Stream> WavConverterAsync(IFormFile sourceFile)
    {
        var stream = new MemoryStream();

        try
        {
            _logger.LogInformation("Convertendo arquivo {FileName} para WAV (codec: pcm_s16le, sample rate: 16000)...", sourceFile.FileName);

            await FFMpegArguments
                .FromPipeInput(new StreamPipeSource(sourceFile.OpenReadStream()))
                .OutputToPipe(new StreamPipeSink(stream),
                    options => options
                        .WithAudioCodec("pcm_s16le")
                        .WithAudioSamplingRate(16000)
                        .ForceFormat("wav"))
                .ProcessAsynchronously()
                .ConfigureAwait(false);

            stream.Seek(0, SeekOrigin.Begin);
            _logger.LogInformation("Conversão concluída com sucesso. Tamanho do WAV: {Length} bytes", stream.Length);
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao converter o arquivo {FileName} para WAV", sourceFile.FileName);
            throw;
        }
    }
}