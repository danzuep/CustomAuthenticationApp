using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomAuthenticationApp.Extensions;

public static class HelperExtensions
{
    internal static UriBuilder GetBaseAddress(this Uri hostBaseAddress)
    {
        var baseAddress = new UriBuilder();
        baseAddress.Scheme = hostBaseAddress.Scheme;
        baseAddress.Host = hostBaseAddress.Host;
        return baseAddress;
    }

    public static HttpClient GetHttpClient(this Uri uri, string? token = null)
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2),
            BaseAddress = uri.GetBaseAddress().Uri
        };
        if (!string.IsNullOrWhiteSpace(token))
            client.AddAuthorization(token);
        return client;
    }

    public static void AddAuthorization(this HttpClient httpClient, string token, string scheme = "Bearer")
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
    }

    public static void AddBasicAuthorization(this HttpClient httpClient, NetworkCredential networkCredential)
    {
        if (networkCredential == null)
            throw new ArgumentNullException(nameof(networkCredential));
        httpClient.AddBasicAuthorization(networkCredential.UserName, networkCredential.Password);
    }

    public static void AddBasicAuthorization(this HttpClient httpClient, string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        httpClient.AddAuthorization(token, "Basic");
    }

    public static async Task DownloadFileAsync(this HttpContent content, string? fileName = null, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = content.Headers.ContentDisposition?.FileName != null ?
                content.Headers.ContentDisposition.FileName : $"{Guid.NewGuid()}:N";
        if (!overwrite && File.Exists(fileName))
            throw new InvalidOperationException($"File {Path.GetFullPath(fileName)} already exists.");
        using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }
    }

    private static readonly string _nl = Environment.NewLine;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public static string ToSerializedString(this object? obj) =>
        obj != null ? JsonSerializer.Serialize(obj, _serializerOptions) : string.Empty;

    public static string ToEnumeratedString<T>(this IEnumerable<T> data, string div = ", ") =>
        data is null ? string.Empty : string.Join(div, data.Select(o => o?.ToString() ?? string.Empty));

    public static void LogResults<T>(this ILogger logger, T? obj, LogLevel logLevel = LogLevel.Information) where T : class =>
        logger.Log(logLevel, "\"{Name}\": {JsonSerializedObject}", typeof(T).Name, obj.ToSerializedString());

    public static string ToJson(this object obj, string? name = null) =>
        string.IsNullOrEmpty(name) ? obj.ToSerializedString() : $"{{{_nl}\"{name}\": {obj.ToSerializedString()}{_nl}}}";
}
