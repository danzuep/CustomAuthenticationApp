namespace CustomAuthenticationApp.Services;

using Microsoft.AspNetCore.Authorization;
using CustomAuthenticationApp.Abstractions;
using CustomAuthenticationApp.Models;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AppAuthenticationStateProvider _stateProvider;

    public AuthenticationService(
        AppAuthenticationStateProvider stateProvider,
        ILogger<AuthenticationService> logger)
    {
        _logger = logger;
        _stateProvider = stateProvider;
    }

    [AllowAnonymous]
    public async Task LoginAsync(LoginCredential loginCredential, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginCredential?.Username) || string.IsNullOrEmpty(loginCredential.Password))
            throw new AuthorizationException("Invalid username or password.");
        _logger.LogDebug("Attempting to login as '{User}'.", loginCredential.Username);
        AppIdentity? identity = null;
        try
        {
            // TODO: call login API
            string apiJwtTokenResponse = AppIdentity.TestJwt;
            identity = new AppIdentity(apiJwtTokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to authenticate user '{Username}'.", loginCredential.Username);
            throw new AuthorizationException($"Authentication did not succeed for user '{loginCredential.Username}'.", ex);
        }
        if (string.IsNullOrWhiteSpace(identity?.Name) || !identity.IsAuthenticated)
            throw new AuthorizationException($"Authentication did not succeed for user '{loginCredential.Username}'.");
        await _stateProvider.LoginAsync(identity);
        //var user = _stateProvider.User;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _stateProvider.LogoutAsync(cancellationToken);
    }
}