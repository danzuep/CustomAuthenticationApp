using CustomAuthenticationApp.Services;

namespace CustomAuthenticationApp.Abstractions
{
    public interface IStorageHandler
    {
        ValueTask<T> InvokeAsync<T>(LocalStorage localStorage, CancellationToken cancellationToken = default);
        ValueTask InvokeVoidAsync(LocalStorage localStorage, CancellationToken cancellationToken = default);
        void SetOptions(StorageAccessorOptions? options);
    }

    public interface IStoreBrowserHandler : IStorageHandler
    {
        Task StartAsync(Action<string?> action);
    }
}