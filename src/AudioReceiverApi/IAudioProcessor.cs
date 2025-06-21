namespace AudioReceiverApi;

public interface IAudioProcessor
{
    Task<Stream> WavConverterAsync(IFormFile sourceFile);
}