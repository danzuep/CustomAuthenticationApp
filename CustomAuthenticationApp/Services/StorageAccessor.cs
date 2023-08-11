namespace CustomAuthenticationApp.Services;

using CustomAuthenticationApp.Abstractions;
using System.Text.Json;

public class StorageAccessor : IStorageAccessor
{
    private readonly IStorageHandler _storageHandler;
    private readonly ILogger<StorageAccessor> _logger;

    public StorageAccessor(IStorageHandler storageHandler, ILogger<StorageAccessor> logger)
    {
        _logger = logger;
        _storageHandler = storageHandler;
    }

    public void SetOptions(BrowserStorageAccessorOptions? options)
    {
        _storageHandler.SetOptions(options);
    }

    internal const string Token = "token";

    public async Task<string?> GetTokenValueAsync(CancellationToken cancellationToken = default) =>
        await GetValueAsync<string?>(Token, cancellationToken);

    public Task SetTokenValueAsync(string? value, DateTime? expiry = null, CancellationToken cancellationToken = default) =>
        SetValueAsync(Token, value, expiry ?? DateTime.UtcNow.AddDays(1), null, cancellationToken);

    private static readonly LocalStorage _get = new(StorageCommand.Get, "key");
    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var storage = _get;
        storage.Args[0] = key;
        try
        {
            T? result;
            if (typeof(T) != typeof(string) && (await _storageHandler.InvokeAsync<string>(storage, cancellationToken)) is string value)
                result = JsonSerializer.Deserialize<T>(value);
            else
                result = await _storageHandler.InvokeAsync<T>(storage, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        return default;
    }

    private static readonly LocalStorage _set = new(StorageCommand.Set, "key", "value", "absoluteExpiryMs", "relativeExpiryMs");
    public async Task SetValueAsync<T>(string key, T? value, DateTime? absoluteExpiry = null, TimeSpan? relativeExpiry = null, CancellationToken cancellationToken = default)
    {
        var storage = _set;
        storage.Args[0] = key;
        storage.Args[1] = value == null || value is string ? value : JsonSerializer.Serialize(value);
        storage.Args[2] = absoluteExpiry;
        if (absoluteExpiry == null)
        {
            relativeExpiry ??= TimeSpan.FromDays(30);
            var expiryDate = DateTime.UtcNow.Add(relativeExpiry.Value.Duration());
            storage.Args[3] = (int)expiryDate.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
        }
        try
        {
            await _storageHandler.InvokeVoidAsync(storage, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private static readonly LocalStorage _delete = new(StorageCommand.Delete, "key");
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var storage = _delete;
        storage.Args[0] = key;
        await _storageHandler.InvokeVoidAsync(storage, cancellationToken);
    }

    public async Task RemoveTokenAsync(CancellationToken cancellationToken = default)
    {
        var storage = _delete;
        storage.Args[0] = Token;
        await _storageHandler.InvokeVoidAsync(storage, cancellationToken);
    }

    private static readonly LocalStorage _clear = new(StorageCommand.Clear);
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        await _storageHandler.InvokeVoidAsync(_clear, cancellationToken);
    }
}

public record StorageAccessorOptions(TimeSpan DefaultExpiration);

public record LocalStorage(StorageCommand Command, params object?[] Args);

public enum StorageCommand
{
    Get,
    Set,
    Delete,
    Clear,
    Listener
}