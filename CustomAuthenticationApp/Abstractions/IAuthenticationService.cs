using CustomAuthenticationApp.Models;
using System.Security.Claims;

namespace CustomAuthenticationApp.Abstractions
{
    public interface IAuthenticationService
    {
        AppIdentity Identity { get; }
        Task LoginAsync(LoginCredential loginCredential, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}