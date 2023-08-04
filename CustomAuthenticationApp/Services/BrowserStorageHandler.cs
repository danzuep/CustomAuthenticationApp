namespace CustomAuthenticationApp.Services;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using CustomAuthenticationApp.Abstractions;

// Inspired by https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared/Extensions/IJSRuntimeExtensions.cs
public class BrowserStorageHandler : IStorageHandler
{
    private readonly IStorageHandler _memoryStorage;
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageHandler(IJSRuntime jsRuntime, IMemoryCache memoryCache)
    {
        _jsRuntime = jsRuntime;
        _memoryStorage = new MemoryStorageHandler(memoryCache);
    }

    private BrowserStorageAccessorOptions StorageOptions =
        new(BrowserStorageType.Cookies, TimeSpan.FromDays(1));

    public void SetOptions(StorageAccessorOptions? options)
    {
        if (options is BrowserStorageAccessorOptions browserStorage)
            StorageOptions = browserStorage;
    }

    public async ValueTask InvokeVoidAsync(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        if (StorageOptions.StorageType == BrowserStorageType.Memory)
            await _memoryStorage.InvokeVoidAsync(localStorage, cancellationToken);
        else
            await SetStorageAsync(localStorage, cancellationToken);
    }

    public async ValueTask<T> InvokeAsync<T>(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        if (localStorage == null)
            throw new ArgumentNullException(nameof(localStorage));
        var result = StorageOptions.StorageType == BrowserStorageType.Memory ?
            await _memoryStorage.InvokeAsync<T>(localStorage, cancellationToken) :
            await GetStorageValueAsync<T>(localStorage, cancellationToken);
        return result;
    }

    private async ValueTask<T> GetStorageValueAsync<T>(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        var method = StorageOptions.StorageType switch
        {
            BrowserStorageType.Cookies => CookieStorageOperators[localStorage.Command],
            BrowserStorageType.Local => LocalStorageOperators[localStorage.Command],
            _ => throw new NotImplementedException()
        };
        var result = await _jsRuntime.InvokeAsync<T>(method, cancellationToken, localStorage.Args);
        return result;
    }

    private async ValueTask SetStorageAsync(LocalStorage localStorage, CancellationToken cancellationToken = default)
    {
        var method = StorageOptions.StorageType switch
        {
            BrowserStorageType.Cookies => CookieStorageOperators[localStorage.Command],
            BrowserStorageType.Local => LocalStorageOperators[localStorage.Command],
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
        };

    private static readonly IDictionary<StorageCommand, string> CookieStorageOperators = new Dictionary<StorageCommand, string>
        {
            {StorageCommand.Get, "cookieStorage.get"},
            {StorageCommand.Set, "cookieStorage.set"},
            {StorageCommand.Delete, "cookieStorage.delete"},
            {StorageCommand.Clear, "cookieStorage.clear"},
        };
}

public record BrowserStorageAccessorOptions(BrowserStorageType StorageType, TimeSpan DefaultExpiration) : StorageAccessorOptions(DefaultExpiration);

public enum BrowserStorageType
{
    Memory,
    Cookies,
    Local,
    Session
}