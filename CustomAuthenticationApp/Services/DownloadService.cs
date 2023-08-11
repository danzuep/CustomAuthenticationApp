namespace CustomAuthenticationApp.Services;

using CustomAuthenticationApp.Abstractions;
using System.Collections.Concurrent;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using CustomAuthenticationApp.Extensions;

/// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-7.0"/>
public class DownloadService : IDownloadService
{
    private readonly IJSRuntime _jsInterop;
    private readonly ILogger<DownloadService> logger;

    public DownloadService(IJSRuntime jsInterop, ILogger<DownloadService> logger)
    {
        _jsInterop = jsInterop;
        this.logger = logger;
    }

    private readonly ConcurrentDictionary<string, HttpClient> _httpClients = new();

    public async ValueTask DownloadFileFromStream(Stream fileStream, string fileName)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException(nameof(fileName));
        fileStream.Position = 0;
        using var streamRef = new DotNetStreamReference(fileStream);
        await _jsInterop.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
        return;
    }

    public async ValueTask TryDownloadFileFromStream(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(fileName))
                await Task.Run(() => DownloadFileFromStream(fileStream, fileName), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to download {fileName}.");
        }
        return;
    }

    public async ValueTask DownloadFileFromUrl(string fileUrl, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentNullException(nameof(fileUrl));
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = Path.GetFileName(fileUrl);
        if (!string.IsNullOrWhiteSpace(fileName))
            await _jsInterop.InvokeVoidAsync("triggerFileDownload", fileName, fileUrl).ConfigureAwait(false);
        return;
    }

    public async ValueTask TryDownloadFileFromUrl(Uri downloadUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug($"Download address is {downloadUrl}.");
            if (!_httpClients.TryGetValue(downloadUrl.Host, out HttpClient? client))
            {
                client = downloadUrl.GetHttpClient();
                _httpClients.TryAdd(downloadUrl.Host, client);
                logger.LogDebug($"HttpClient added for {client.BaseAddress}.");
            }
            using var requestMessage = new HttpRequestMessage(HttpMethod.Head, downloadUrl.PathAndQuery);
            logger.LogTrace($"HttpClient request Uri is {requestMessage.RequestUri}.");
            using var responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            await Task.Run(() => DownloadFileFromUrl(downloadUrl.OriginalString), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Automatic download of {Url} failed.", downloadUrl);
            var url = downloadUrl.ToString();
            logger.LogInformation($"Opening a new tab to {url}.");
            await _jsInterop.InvokeVoidAsync("open", url, "_blank").ConfigureAwait(false);
        }
        return;
    }

    public static async ValueTask<Exception?> TryDownloadFileFromUrl(Uri downloadUrl, HttpClient? client = null, string? jwt = null, CancellationToken cancellationToken = default)
    {
        Exception? exception = null;
        try
        {
            client ??= downloadUrl.GetHttpClient(jwt);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, downloadUrl.PathAndQuery);
            using var responseMessage = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            responseMessage.EnsureSuccessStatusCode();
            await responseMessage.Content.DownloadFileAsync(null, true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        return exception;
    }
}
