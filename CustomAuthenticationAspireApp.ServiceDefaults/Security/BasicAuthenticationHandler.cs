namespace CustomAuthenticationAspireApp.ServiceDefaults.Security;

using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static readonly string Name = "Basic";
    public static readonly string SchemeCsv = $"{Name},{NegotiateDefaults.AuthenticationScheme}";

    private readonly IBasicAuthenticationService _authenticationService;

    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IBasicAuthenticationService authenticationService)
        : base(options, logger, encoder)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;

        if (!Request.Headers.ContainsKey("Authorization"))
        {
            if (Context.User.Identity?.IsAuthenticated == true)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name));
            }
            if (!_authenticationService.IsAuthenticationRequired(Context))
            {
                return AuthenticateResult.NoResult();
            }
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var basicPrefix = "Basic ";
            var authHeader = Request.Headers.Authorization.ToString();
            var authHeaderValue = authHeader.StartsWith(basicPrefix, StringComparison.OrdinalIgnoreCase)
                ? authHeader[basicPrefix.Length..].Trim()
                : string.Empty;

            var credentialBytes = Convert.FromBase64String(authHeaderValue);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            if (credentials.Length != 2)
                return AuthenticateResult.Fail("Invalid Credentials Format");
            var username = credentials[0];
            var password = credentials[1];

            // Validate credentials (Replace with your logic)
            if (_authenticationService.IsAuthenticated(username, password))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await Task.CompletedTask;
        if (_authenticationService.IsChallengeRequired(Context.Connection.LocalPort))
        {
            Response.Headers.WWWAuthenticate = $"Basic realm=\"{nameof(CustomAuthenticationAspireApp)}\"";
        }
        else if (_authenticationService.IsAuthenticationRequired(Context))
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = MediaTypeNames.Text.Plain;
            await Response.WriteAsync("Challenge failed");
        }
        else
        {
            Response.StatusCode = StatusCodes.Status200OK;
        }
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        if (_authenticationService.IsChallengeRequired(Context.Connection.LocalPort) ||
            _authenticationService.IsAuthenticationRequired(Context))
        {
            await base.HandleForbiddenAsync(properties);
            Response.ContentType = MediaTypeNames.Text.Plain;
            await Response.WriteAsync("Forbidden");
            return;
        }

        Response.StatusCode = StatusCodes.Status200OK;
    }

    //public static string CreateToken(string secret, string name)
    //{
    //    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
    //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    //    var token = new JwtSecurityToken(
    //        claims: [new Claim(ClaimTypes.Name, name)],
    //        signingCredentials: creds,
    //        expires: DateTime.UtcNow.AddMinutes(15)
    //        );
    //    return new JwtSecurityTokenHandler().WriteToken(token);
    //}
}