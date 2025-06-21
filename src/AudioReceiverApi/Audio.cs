namespace AudioReceiverApi;

public class Audio
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string SourceFileName { get; set; }
    public string SourceFileContentType { get; set; }
    public string MinioFilePath { get; set; }
    public string TranscriptionWithoutTimestamp { get; set; }
    public string TrascriptionWithTimestamp { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string MinioEtag { get; set; }
}