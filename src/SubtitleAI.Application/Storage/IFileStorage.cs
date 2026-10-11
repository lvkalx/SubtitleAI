namespace SubtitleAI.Application.Storage;

public interface IFileStorage
{
    Task SaveAsync(string blobName, Stream content, string contentType, CancellationToken ct);
    Task<Stream> OpenReadAsync(string blobName, CancellationToken ct);
    Task DeleteAsync(string blobName, CancellationToken ct);
}