namespace CustomAuthenticationApp.Services;

using Microsoft.AspNetCore.Authorization;
using CustomAuthenticationApp.Abstractions;
using CustomAuthenticationApp.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

internal sealed class AppAuthenticationService : IAuthenticationService
{
    private readonly ILogger<AppAuthenticationService> _logger;
    private readonly IWebAssemblyHostEnvironment _environment;
    private readonly AppAuthenticationStateProvider _stateProvider;

    public AppAuthenticationService(
        AppAuthenticationStateProvider stateProvider,
        IWebAssemblyHostEnvironment hostEnvironment,
        ILogger<AppAuthenticationService> logger)
    {
        _logger = logger;
        _environment = hostEnvironment;
        _stateProvider = stateProvider;
    }

    public AppIdentity Identity => _stateProvider.Identity;

    [AllowAnonymous]
    public async Task LoginAsync(LoginCredential loginCredential, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginCredential?.Username) || string.IsNullOrEmpty(loginCredential.Password))
            throw new AuthorizationException("Invalid username or password.");
        _logger.LogDebug("Attempting to login as \"{User}\".", loginCredential.Username);
        AppIdentity? identity;
        try
        {
            // TODO: call login API
            string? apiJwtTokenResponse = _environment.IsDevelopment() ? AppIdentity.TestJwt : null;
            identity = new AppIdentity(apiJwtTokenResponse, loginCredential.Username);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to authenticate user \"{Username}\".", loginCredential.Username);
            throw new AuthorizationException($"Authentication did not succeed for user \"{loginCredential.Username}\".", ex);
        }
        await _stateProvider.LoginAsync(identity, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _stateProvider.LogoutAsync(cancellationToken);
    }
}