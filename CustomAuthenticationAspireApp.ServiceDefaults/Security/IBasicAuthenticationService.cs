using Microsoft.AspNetCore.Http;

namespace CustomAuthenticationAspireApp.ServiceDefaults.Security
{
    public interface IBasicAuthenticationService
    {
        bool IsAuthenticated(string username, string password);

        bool IsAuthenticationRequired(HttpContext context);

        bool IsChallengeRequired(int port);
    }
}