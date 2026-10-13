using System.Net;
using System.Net.Sockets;

namespace SubtitleAI.Infrastructure.Jobs;

public interface IUrlMediaDownloader
{
    Task<DownloadedMedia> DownloadAsync(string sourceUrl, CancellationToken ct);
}

public sealed class DownloadedMedia(string path, string fileName, string contentType) : IAsyncDisposable
{
    public string FileName { get; } = fileName;
    public string ContentType { get; } = contentType;

    public FileStream OpenRead() => new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    public ValueTask DisposeAsync()
    {
        if (File.Exists(path)) File.Delete(path);
        return ValueTask.CompletedTask;
    }
}

public sealed class RemoteMediaException(string errorKey) : Exception(errorKey)
{
    public string ErrorKey { get; } = errorKey;
}

public sealed class UrlMediaDownloader(HttpClient client) : IUrlMediaDownloader
{
    private const long MaxBytes = 500L * 1024 * 1024;
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".ogg", ".flac", ".aac", ".mp4", ".mkv", ".mov", ".webm", ".avi"
    };

    public async Task<DownloadedMedia> DownloadAsync(string sourceUrl, CancellationToken ct)
    {
        if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new RemoteMediaException("invalidMediaUrl");

        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!response.IsSuccessStatusCode) throw new RemoteMediaException("mediaUrlUnavailable");
        if (response.Content.Headers.ContentLength > MaxBytes) throw new RemoteMediaException("mediaUrlTooLarge");

        var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant() ?? "application/octet-stream";
        var fileName = Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath));
        var extension = Path.GetExtension(fileName);
        if (!Extensions.Contains(extension) && !contentType.StartsWith("audio/") && !contentType.StartsWith("video/"))
            throw new RemoteMediaException("mediaUrlUnsupported");
        if (string.IsNullOrWhiteSpace(fileName) || !Extensions.Contains(extension))
            fileName = $"remote-media{ExtensionFor(contentType)}";

        var tempPath = Path.Combine(Path.GetTempPath(), $"subtitleai-{Guid.NewGuid():N}{Path.GetExtension(fileName)}");
        try
        {
            await using var input = await response.Content.ReadAsStreamAsync(ct);
            await using var output = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            var buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                var read = await input.ReadAsync(buffer, ct);
                if (read == 0) break;
                total += read;
                if (total > MaxBytes) throw new RemoteMediaException("mediaUrlTooLarge");
                await output.WriteAsync(buffer.AsMemory(0, read), ct);
            }
            return new DownloadedMedia(tempPath, fileName, contentType);
        }
        catch
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            throw;
        }
    }

    public static SocketsHttpHandler CreateSafeHandler() => new()
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 3,
        ConnectCallback = async (context, ct) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);
            var address = addresses.FirstOrDefault(IsPublicAddress);
            if (address is null) throw new RemoteMediaException("mediaUrlUnavailable");

            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), ct);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };

    private static bool IsPublicAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.IsIPv6LinkLocal || address.IsIPv6Multicast || address.IsIPv6SiteLocal)
            return false;
        var bytes = address.MapToIPv4().GetAddressBytes();
        return !(bytes[0] == 0 || bytes[0] == 10 || bytes[0] == 127 || bytes[0] >= 224 ||
                 (bytes[0] == 169 && bytes[1] == 254) ||
                 (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                 (bytes[0] == 192 && bytes[1] == 168));
    }

    private static string ExtensionFor(string contentType) => contentType switch
    {
        "audio/mpeg" => ".mp3", "audio/wav" or "audio/x-wav" => ".wav", "audio/mp4" => ".m4a",
        "audio/ogg" => ".ogg", "video/webm" or "audio/webm" => ".webm", "video/quicktime" => ".mov",
        "video/x-msvideo" => ".avi", _ => ".mp4"
    };
}
