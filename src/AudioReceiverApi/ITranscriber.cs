namespace AudioReceiverApi;

public interface ITranscriber
{
    Task<(string WithoutTimestamp, string WithTimestamp)> ProcessAsync(Stream stream);
}