namespace CustomAuthenticationApp.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using CustomAuthenticationApp.Abstractions;
using CustomAuthenticationApp.Models;

// https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/?view=aspnetcore-7.0&tabs=visual-studio#implement-a-custom-authenticationstateprovider
// https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?pivots=webassembly&view=aspnetcore-7.0#browser-storage-wasm
public sealed class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IStorageAccessor _browserStorage;
    private readonly ILogger<AppAuthenticationStateProvider> _logger;

    public AppAuthenticationStateProvider(IStorageAccessor browserStorage, ILogger<AppAuthenticationStateProvider> logger)
    {
        _logger = logger;
        _browserStorage = browserStorage;
    }

    private static readonly AppIdentity _defaultIdentity = new();
    private static readonly ClaimsIdentity _defaultClaimsIdentity = new();
    private static readonly ClaimsPrincipal _defaultClaimsPrincipal = new(_defaultClaimsIdentity);
    private static readonly AuthenticationState _defaultAuthenticationState = new(_defaultClaimsPrincipal);
    private static readonly Task<AuthenticationState> _defaultAuthenticationStateTask = Task.FromResult(_defaultAuthenticationState);

    private AppIdentity _identity = _defaultIdentity;
    internal AppIdentity Identity => _identity;
    private ClaimsPrincipal _claimsPrincipal => _identity.User;
    private AuthenticationState _authenticationState => new(_claimsPrincipal);
    public string? Name => _identity.Name;

    public async Task<AppIdentity> GetAppIdentityAsync(CancellationToken cancellationToken = default) =>
        new AppIdentity(await _browserStorage.GetTokenValueAsync(cancellationToken));

    public Task SetTokenValueAsync(AppIdentity? appIdentity, CancellationToken cancellationToken = default) =>
        _browserStorage.SetTokenValueAsync(appIdentity?.Jwt, appIdentity?.ValidTo, cancellationToken);

    private async Task<bool> IsTokenExpiredAsync()
    {
        _identity = await GetAppIdentityAsync() ?? _defaultIdentity;
        if (_identity == null || !_identity.IsAuthenticated || _identity.Error != null)
        {
            var isExpired = _identity?.GetIsExpiredFromClaimsIdentity();
            if (isExpired.HasValue && isExpired.Value == true)
            {
                if (isExpired.HasValue)
                    _logger.LogDebug("Authentication has expired for user \"{Name}\".", Name);
                _logger.LogInformation(_identity?.Error, "Authentication did not succeed for user \"{Name}\".", _identity?.Name);
                _identity = _defaultIdentity;
                await _browserStorage.RemoveTokenAsync(CancellationToken.None);
                return true;
            }
        }
        _logger.LogDebug("{Name} authenticated.", Name);
        return false;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_identity.IsAuthenticated && await IsTokenExpiredAsync())
            return _defaultAuthenticationState;
        return _authenticationState;
    }

    public async Task LoginAsync(AppIdentity identity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identity?.Name))
            throw new AuthorizationException("Authentication did not succeed, no name was supplied.");
        string name = identity.Name;
        _logger.LogDebug("User attempting to login: {Name}.", name);
        if (!identity.IsAuthenticated)
            throw new AuthorizationException($"Authentication did not succeed for user \"{name}\".");
        var claimsIdentity = identity.ClaimsIdentity;
        if (claimsIdentity == null)
            throw new AuthorizationException("Authentication did not succeed, not a user.");
        if (!claimsIdentity.IsAuthenticated)
            throw new AuthorizationException($"Authentication did not succeed for user \"{name}\".");
        if (string.IsNullOrWhiteSpace(claimsIdentity?.Name))
            throw new AuthorizationException("Authentication did not succeed, user claim has no name.");
        var isExpired = identity.GetIsExpiredFromClaimsIdentity();
        if (!isExpired.HasValue)
            _logger.LogDebug("Authentication identity has no expiry for user \"{Name}\".", name);
        else if (isExpired != false)
            throw new AuthorizationException($"Authentication did not succeed for user \"{name}\", token expired.");
        _identity = identity;
        await SetTokenValueAsync(identity, cancellationToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        _identity = _defaultIdentity;
        await _browserStorage.RemoveTokenAsync(cancellationToken);
        NotifyAuthenticationStateChanged(_defaultAuthenticationStateTask);
    }
}
