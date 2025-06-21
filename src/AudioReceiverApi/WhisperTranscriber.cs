using System.Text;
using Microsoft.Extensions.Logging;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.LibraryLoader;

namespace AudioReceiverApi;

public sealed class WhisperTranscriber : ITranscriber
{
    private readonly string _path;
    private readonly bool _isDev;
    private readonly ILogger<WhisperTranscriber> _logger;

    public WhisperTranscriber(IConfiguration config, IWebHostEnvironment env, ILogger<WhisperTranscriber> logger)
    {
        _path = config["GGML_PATH"]!;
        _isDev = env.IsDevelopment();
        _logger = logger;
    }

    public async Task<(string WithoutTimestamp, string WithTimestamp)> ProcessAsync(Stream stream)
    {
        try
        {
            _logger.LogInformation("Iniciando transcrição com Whisper.");

            RuntimeOptions.RuntimeLibraryOrder =
            [
                RuntimeLibrary.Cuda,
                RuntimeLibrary.Vulkan,
                RuntimeLibrary.Cpu,
                RuntimeLibrary.CpuNoAvx
            ];

            if (!File.Exists(_path))
            {
                _logger.LogWarning("Modelo Whisper não encontrado em {Path}. Baixando modelo LargeV3...", _path);
                await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.LargeV3);
                await using var fileWriter = File.OpenWrite(_path);
                await modelStream.CopyToAsync(fileWriter);
                _logger.LogInformation("Modelo Whisper baixado e salvo em {Path}.", _path);
            }

            using var factory = WhisperFactory.FromPath(_path);
            await using var processor = factory.CreateBuilder()
                                               .WithLanguage("pt")
                                               .Build();

            stream.Seek(0, SeekOrigin.Begin);

            var semTempo = new StringBuilder();
            var comTempo = new StringBuilder();

            await foreach (var result in processor.ProcessAsync(stream))
            {
                comTempo.AppendLine($"{result.Start}-{result.End}-{result.Text}");
                semTempo.AppendLine(result.Text);
            }

            _logger.LogInformation("Transcrição concluída com sucesso. Total de linhas: {Linhas}", semTempo.ToString().Split('\n').Length);
            return (semTempo.ToString(), comTempo.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o processo de transcrição com Whisper.");
            throw;
        }
    }
}
