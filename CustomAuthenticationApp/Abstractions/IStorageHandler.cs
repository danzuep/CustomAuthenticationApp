using CustomAuthenticationApp.Services;

namespace CustomAuthenticationApp.Abstractions
{
    public interface IStorageHandler
    {
        ValueTask<T> InvokeAsync<T>(BrowserStorage localStorage, CancellationToken cancellationToken = default);
        ValueTask InvokeVoidAsync(BrowserStorage localStorage, CancellationToken cancellationToken = default);
        void SetOptions(StorageAccessorOptions? options);
    }

    public interface IStoreBrowserHandler : IStorageHandler
    {
        void SetOptions(StorageType storageType, TimeSpan? timeSpan = null);
        Task StartAsync(Action<string?> action);
    }
}