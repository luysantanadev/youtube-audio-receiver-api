namespace AudioReceiverApi;

public interface IAudioRepository
{
    Task SaveAsync(Audio audio);
}