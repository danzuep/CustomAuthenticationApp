using CustomAuthenticationApp.Models;

namespace CustomAuthenticationApp.Abstractions
{
    public interface IAuthenticationService
    {
        Task LoginAsync(LoginCredential loginCredential, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}