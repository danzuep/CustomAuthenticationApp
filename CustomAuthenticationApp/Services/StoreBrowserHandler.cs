namespace CustomAuthenticationApp.Services;

using CustomAuthenticationApp.Abstractions;
using Microsoft.JSInterop;

// Inspired by https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared/Extensions/IJSRuntimeExtensions.cs
public class StoreBrowserHandler : IStoreBrowserHandler
{
    private Action<string?>? _action;
    private readonly IStorageHandler _memoryStorage;
    private readonly IJSRuntime _jsRuntime;

    public StoreBrowserHandler(IJSRuntime jsRuntime, IStorageHandler memoryStorage)
    {
        _jsRuntime = jsRuntime;
        _memoryStorage = memoryStorage;
    }

    //private Lazy<IStorageHandler> _storage =>
    //    new Lazy<IStorageHandler>(() => _storageOptions.StorageType switch
    //    {
    //        StorageType.Local => new StoreLocalHandler(_jsRuntime),
    //        StorageType.Cookies => new StoreCookiesHandler(_jsRuntime),
    //        _ => new StoreMemoryHandler(_memoryCache)
    //    });

    private BrowserStorageAccessorOptions _storageOptions =
        new(StorageType.Cookies, TimeSpan.FromDays(1));

    public void SetOptions(StorageType storageType, TimeSpan? timeSpan = null)
    {
        _storageOptions = new(storageType, timeSpan ?? _storageOptions.DefaultExpiration);
    }

    public void SetOptions(StorageAccessorOptions? options)
    {
        if (options is BrowserStorageAccessorOptions browserStorage)
            _storageOptions = browserStorage;
    }

    /// <summary>
    /// Subscribe to the storage/cookie event. This will do work when a key-value changes.
    /// </summary>
    public async Task StartAsync(Action<string?> action)
    {
        if (action != null)
            _action = action;
        //var initialValue = await GetStorageValueAsync<string?>(new(StorageCommand.Get));
        var jsEventMethod = _storageOptions.StorageType switch
        {
            StorageType.Cookies => CookieStorageOperators[StorageCommand.Listener],
            StorageType.Local => LocalStorageOperators[StorageCommand.Listener],
            _ => throw new NotImplementedException()
        };
        // Create a reference to the current object
        var reference = DotNetObjectReference.Create(this);
        // Create a task that will call the JS "eventListener" function when run.
        // That function looks for the [JSInvokable] dotnet method "OnStorageUpdated".
        await _jsRuntime.InvokeVoidAsync(jsEventMethod, reference);
    }

    // This method will be called when a key-value changes.
    [JSInvokable]
    public Task OnStorageUpdated(string? changedKey)
    {
        // Handle the changes by invoking the action, e.g. get the new value
        _action?.Invoke(changedKey);
        return Task.CompletedTask;
    }

    public async ValueTask InvokeVoidAsync(BrowserStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        if (_storageOptions.StorageType == StorageType.Memory)
            await _memoryStorage.InvokeVoidAsync(localStorage, cancellationToken);
        else
            await SetStorageAsync(localStorage, cancellationToken);
    }

    public async ValueTask<T> InvokeAsync<T>(BrowserStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        var result = _storageOptions.StorageType == StorageType.Memory ?
            await _memoryStorage.InvokeAsync<T>(localStorage, cancellationToken) :
            await GetStorageValueAsync<T>(localStorage, cancellationToken);
        return result;
    }

    private async ValueTask<T> GetStorageValueAsync<T>(BrowserStorage localStorage, CancellationToken cancellationToken = default)
    {
        var method = _storageOptions.StorageType switch
        {
            StorageType.Cookies => CookieStorageOperators[localStorage.Command],
            StorageType.Local => LocalStorageOperators[localStorage.Command],
            _ => throw new NotImplementedException()
        };
        var result = await _jsRuntime.InvokeAsync<T>(method, cancellationToken, localStorage.Args);
        return result;
    }

    private async ValueTask SetStorageAsync(BrowserStorage localStorage, CancellationToken cancellationToken = default)
    {
        var method = _storageOptions.StorageType switch
        {
            StorageType.Cookies => CookieStorageOperators[localStorage.Command],
            StorageType.Local => LocalStorageOperators[localStorage.Command],
            _ => throw new NotImplementedException()
        };
        await _jsRuntime.InvokeVoidAsync(method, cancellationToken, localStorage.Args);
    }

    private static readonly IDictionary<StorageCommand, string> LocalStorageOperators = new Dictionary<StorageCommand, string>
        {
            {StorageCommand.Get, "localStorage.getItem"},
            {StorageCommand.Set, "localStorage.setItem"},
            {StorageCommand.Delete, "localStorage.removeItem"},
            {StorageCommand.Clear, "localStorage.clear"},
            {StorageCommand.Listener, "localStorage.eventListener"},
        };

    private static readonly IDictionary<StorageCommand, string> CookieStorageOperators = new Dictionary<StorageCommand, string>
        {
            {StorageCommand.Get, "cookieStorage.get"},
            {StorageCommand.Set, "cookieStorage.set"},
            {StorageCommand.Delete, "cookieStorage.delete"},
            {StorageCommand.Clear, "cookieStorage.clear"},
            {StorageCommand.Listener, "cookieStorage.eventListener"},
        };
}

public record BrowserStorageAccessorOptions(StorageType StorageType, TimeSpan DefaultExpiration) : StorageAccessorOptions(DefaultExpiration);

public enum StorageType
{
    Memory,
    Cookies,
    Local,
    Session
}