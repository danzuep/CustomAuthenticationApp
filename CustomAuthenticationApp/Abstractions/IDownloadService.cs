namespace CustomAuthenticationApp.Abstractions
{
    public interface IDownloadService
    {
        /// <summary>
        /// Stream file content to a raw binary data buffer on the client.
        /// Typically, this approach is used for relatively small files (< 250 MB).
        /// </summary>
        ValueTask TryDownloadFileFromStream(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a file via a URL without streaming.
        /// Usually, this approach is used for relatively large files (> 250 MB).
        /// </summary>
        ValueTask TryDownloadFileFromUrl(Uri downloadUrl, CancellationToken cancellationToken = default);
    }
}