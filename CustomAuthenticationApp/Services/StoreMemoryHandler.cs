namespace CustomAuthenticationApp.Services;

using CustomAuthenticationApp.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

public class StoreMemoryHandler : IStorageHandler, IDisposable
{
    private readonly IMemoryCache _memoryCache;

    public StoreMemoryHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    private StorageAccessorOptions StorageOptions =
        new(TimeSpan.FromMinutes(10));

    public void SetOptions(StorageAccessorOptions? options)
    {
        if (options != null)
            StorageOptions = options;
    }

    public ValueTask<T> InvokeAsync<T>(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        var result = GetMemoryCacheValue<T>(localStorage);
        return ValueTask.FromResult(result);
    }

    public ValueTask InvokeVoidAsync(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        SetMemoryCache(localStorage);
        return ValueTask.CompletedTask;
    }

    private T GetMemoryCacheValue<T>(LocalStorage localStorage)
    {
        var key = localStorage.Args[0];
        if (localStorage.Args == null)
            throw new ArgumentException("No arguments were provided.");
        else if (key is not object)
            throw new ArgumentException("Key must be a valid object.");
        else
        {
            if (!_memoryCache.TryGetValue(key, out T? cachedValue))
                throw new KeyNotFoundException($"Key not found: {key}.");
            else if (cachedValue == null)
                throw new ArgumentException($"Key value type conversion failed for: {key}.");
            return cachedValue;
        }
    }

    private CancellationTokenSource _resetCacheToken = new();
    private void SetMemoryCache(LocalStorage localStorage)
    {
        if (localStorage.Command == StorageCommand.Clear)
        {
            if (_resetCacheToken != null &&
                !_resetCacheToken.IsCancellationRequested &&
                _resetCacheToken.Token.CanBeCanceled)
            {
                _resetCacheToken.Cancel();
                _resetCacheToken.Dispose();
            }
            _resetCacheToken = new CancellationTokenSource();
            return;
        }
        if (localStorage.Args == null)
            throw new ArgumentException("No arguments were provided.");
        if (localStorage.Args.Length < 1)
            throw new ArgumentException("No key argument was specified.");
        var key = localStorage.Args[0];
        if (key is not object)
            throw new ArgumentException("Key must be a valid object.");
        if (localStorage.Command == StorageCommand.Delete)
        {
            _memoryCache.Remove(key);
            return;
        }

        if (localStorage.Args.Length > 3)
        {
            var value = localStorage.Args[1];
            var cancellationToken = new CancellationChangeToken(_resetCacheToken.Token);
            var options = new MemoryCacheEntryOptions().AddExpirationToken(cancellationToken);
            if (localStorage.Args[2] is DateTime absoluteExpiration)
                options.SetAbsoluteExpiration(absoluteExpiration);
            else if (localStorage.Args[3] is TimeSpan relativeExpiration)
                options.SetAbsoluteExpiration(relativeExpiration);
            else
                options.SetAbsoluteExpiration(StorageOptions.DefaultExpiration);
            _memoryCache.Set(key, value, options);
        }
        else if (localStorage.Args.Length > 2)
        {
            var value = localStorage.Args[1];
            var absoluteExpiration = localStorage.Args[2] is DateTime expiry ?
                expiry : DateTime.UtcNow.Add(StorageOptions.DefaultExpiration);
            var cancellationToken = new CancellationChangeToken(_resetCacheToken.Token);
            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(cancellationToken)
                .SetAbsoluteExpiration(absoluteExpiration);
            _memoryCache.Set(key, value, options);
        }
        else if (localStorage.Args.Length > 1)
        {
            var value = localStorage.Args[1];
            var cancellationToken = new CancellationChangeToken(_resetCacheToken.Token);
            var options = new MemoryCacheEntryOptions().AddExpirationToken(cancellationToken);
            options.SetAbsoluteExpiration(StorageOptions.DefaultExpiration);
            _memoryCache.Set(key, value, options);
        }
        else
        {
            throw new ArgumentException("No value argument was specified.");
        }
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }
}