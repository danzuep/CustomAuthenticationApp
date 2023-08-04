namespace CustomAuthenticationApp.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
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

    private static readonly ClaimsIdentity _defaultClaimsIdentity = new();
    private static readonly ClaimsPrincipal _defaultClaimsPrincipal = new(_defaultClaimsIdentity);
    private static readonly AuthenticationState _defaultAuthenticationState = new(_defaultClaimsPrincipal);
    private static readonly Task<AuthenticationState> _defaultAuthenticationStateTask = Task.FromResult(_defaultAuthenticationState);

    public string? Name => _claimsIdentity.Name;
    private ClaimsIdentity _claimsIdentity = _defaultClaimsIdentity;
    private ClaimsPrincipal _claimsPrincipal => new(_claimsIdentity);
    private AuthenticationState _authenticationState => new(_claimsPrincipal);
    public ClaimsPrincipal User => _claimsPrincipal;

    private AppIdentity? _identity;
    internal IIdentity Identity => _identity ?? new();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_claimsIdentity.IsAuthenticated)
        {
            var appIdentity = await GetAppIdentityAsync();
            if (appIdentity == null || !appIdentity.IsAuthenticated || appIdentity.Error != null)
            {
                _logger.LogInformation(appIdentity?.Error, "Authentication did not succeed for user '{Name}'.", appIdentity?.Name);
                _claimsIdentity = _defaultClaimsIdentity;
                return _defaultAuthenticationState;
            }
            _claimsIdentity = appIdentity.ClaimsIdentity;
        }
        var expiryClaim = _claimsIdentity.FindFirst(JwtRegisteredClaimNames.Exp) ??
            _claimsIdentity.FindFirst(ClaimTypes.Expiration);
        var expiry = double.TryParse(expiryClaim?.Value, out double maxAge) ?
            DateTime.UnixEpoch.AddSeconds(maxAge) : DateTime.UtcNow.AddDays(1);
        if (expiryClaim == null || DateTime.UtcNow > expiry)
        {
            _logger.LogDebug("Authentication has expired for user '{Name}'.", Name);
            _claimsIdentity = _defaultClaimsIdentity;
            await _browserStorage.RemoveTokenAsync(CancellationToken.None);
            return _defaultAuthenticationState;
        }
        _logger.LogDebug("{Name} authenticated.", Name);
        return _authenticationState;
    }

    public async Task<AppIdentity> GetAppIdentityAsync(CancellationToken cancellationToken = default) =>
        new AppIdentity(await _browserStorage.GetTokenValueAsync(cancellationToken));

    public Task SetAppIdentityAsync(AppIdentity? appIdentity, CancellationToken cancellationToken = default) =>
        _browserStorage.SetTokenValueAsync(appIdentity?.Jwt, appIdentity?.ValidTo, cancellationToken);

    private void Login(ClaimsIdentity? claimsIdentity)
    {
        if (string.IsNullOrWhiteSpace(claimsIdentity?.Name))
            throw new AuthorizationException("Authentication did not succeed, no name was given.");
        _claimsIdentity = claimsIdentity;
        _logger.LogDebug("User attempting to login: {User}.", Name);
        if (!_claimsIdentity.IsAuthenticated || Name == null)
            throw new AuthorizationException($"Authentication did not succeed for user '{Name}'.");
        var expiryClaim = _claimsIdentity.FindFirst(JwtRegisteredClaimNames.Exp) ??
            _claimsIdentity.FindFirst(ClaimTypes.Expiration);
        var expiry = double.TryParse(expiryClaim?.Value, out double maxAge) ?
            DateTime.UnixEpoch.AddSeconds(maxAge) : DateTime.UtcNow.AddDays(1);
        //await _browserStorage.SetTokenValueAsync(_claimsIdentity, expiry, cancellationToken);
        if (expiryClaim == null || DateTime.UtcNow > expiry)
            throw new AuthorizationException($"Authentication no longer valid for user '{Name}'.");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Login(IIdentity? identity, IEnumerable<Claim>? claims = null)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));
        if (string.IsNullOrWhiteSpace(identity?.Name))
            throw new AuthorizationException("Authentication did not succeed, no name was given.");
        if (identity is ClaimsIdentity claimsIdentity)
            _claimsIdentity = claimsIdentity;
        else if (claims != null)
            _claimsIdentity = new ClaimsIdentity(identity, claims);
        else
            _claimsIdentity = new(identity);
        Login(_claimsIdentity);
    }

    public async Task LoginAsync(AppIdentity identity, CancellationToken cancellationToken = default)
    {
        await SetAppIdentityAsync(identity, cancellationToken);
        Login(identity?.ClaimsIdentity);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        _claimsIdentity = _defaultClaimsIdentity;
        await _browserStorage.RemoveTokenAsync(cancellationToken);
        NotifyAuthenticationStateChanged(_defaultAuthenticationStateTask);
    }
}
