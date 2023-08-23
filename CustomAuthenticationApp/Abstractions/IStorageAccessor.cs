using CustomAuthenticationApp.Services;

namespace CustomAuthenticationApp.Abstractions
{
    public interface IStorageAccessor
    {
        Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<string?> GetTokenValueAsync(CancellationToken cancellationToken = default);
        Task SetValueAsync<T>(string key, T value, DateTime? absoluteExpiry = null, TimeSpan? relativeExpiry = null, CancellationToken cancellationToken = default);
        Task SetTokenValueAsync(string? value, DateTime? expiry = null, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task RemoveTokenAsync(CancellationToken cancellationToken = default);
        Task ClearAllAsync(CancellationToken cancellationToken = default);
        void SetOptions(BrowserStorageAccessorOptions? options);
        void SetOptions(StorageType storageType, TimeSpan? timeSpan = null);
    }
}