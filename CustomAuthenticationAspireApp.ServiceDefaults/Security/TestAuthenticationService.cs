using Microsoft.AspNetCore.Http;

namespace CustomAuthenticationAspireApp.ServiceDefaults.Security
{
    public class TestAuthenticationService : IBasicAuthenticationService
    {
        public bool IsAuthenticated(string username, string password)
        {
            return username == "test" || username == "t";
        }

        public bool IsChallengeRequired(int port) =>
            port % 10 == 0;

        public bool IsAuthenticationRequired(HttpContext context)
        {
            var excludedPaths = new[] { "/index.html", ".ico" };

            // bypass authentication for specific paths
            if (!context.Request.Path.HasValue ||
                context.Request.Path.Value == "/" ||
                context.Request.Path.Value == "/health" ||
                context.Request.Path.Value.Contains("scalar", StringComparison.OrdinalIgnoreCase) ||
                excludedPaths.Any(p => context.Request.Path.Value.EndsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }
    }
}
