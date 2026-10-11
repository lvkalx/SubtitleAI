using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SubtitleAI.Application.Storage;

namespace SubtitleAI.Infrastructure.Storage;

public class StorageOptions
{
    public string ConnectionString { get; set; } = default!;
    public string Container { get; set; } = "media";
}

public class BlobFileStorage(BlobServiceClient service, IOptions<StorageOptions> options) : IFileStorage
{
    private readonly BlobContainerClient _container = service.GetBlobContainerClient(options.Value.Container);

    public async Task SaveAsync(string blobName, Stream content, string contentType, CancellationToken ct)
    {
        await _container.CreateIfNotExistsAsync(cancellationToken: ct);
        await _container.GetBlobClient(blobName).UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            ct);
    }

    public async Task<Stream> OpenReadAsync(string blobName, CancellationToken ct) =>
        await _container.GetBlobClient(blobName).OpenReadAsync(cancellationToken: ct);

    public async Task DeleteAsync(string blobName, CancellationToken ct) =>
        await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
}