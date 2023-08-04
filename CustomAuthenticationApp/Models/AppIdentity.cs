namespace CustomAuthenticationApp.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

public record AppIdentity : IIdentity
{
    internal static readonly string TestJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

    protected static readonly ClaimsIdentity _defaultClaimsIdentity = new();
    protected ClaimsIdentity _claimsIdentity = _defaultClaimsIdentity;
    public ClaimsIdentity ClaimsIdentity => _claimsIdentity;

    protected JwtSecurityToken? _token;

    protected readonly List<Claim> _claims = new();
    public IEnumerable<Claim> Claims => _claims;
    public virtual IEnumerable<string> Roles => _claims
        .Where(claim => claim.Type == ClaimTypes.Role)
        .Select(c => c.Value);

    /// <summary>
    /// Gets the ValidFrom "not before" date.
    /// JwtRegisteredClaimNames.Nbf
    /// </summary>
    public virtual DateTime? ValidFrom { get; protected set; }

    /// <summary>
    /// Gets the ValidTo expiration date.
    /// JwtRegisteredClaimNames.Exp
    /// </summary>
    public virtual DateTime? ValidTo { get; protected set; }

    /// <summary>
    /// Gets a value that indicates if the token is valid.
    /// </summary>
    public virtual bool HasValidDates =>
        (ValidFrom == null || DateTime.UtcNow >= ValidFrom) &&
        (ValidTo == null || DateTime.UtcNow < ValidTo);

    /// <summary>
    /// Gets a value that indicates if the user has been authenticated.
    /// IsAuthenticated will be false if ClaimTypes.Name or similar is not set.
    /// </summary>
    public virtual bool IsAuthenticated =>
        _claimsIdentity.IsAuthenticated && IsValidated && HasValidDates;

    /// <summary>
    /// Gets the Name of this <see cref="AppIdentity"/>.
    /// </summary>
    public virtual string? Name => _claimsIdentity.Name;

    /// <summary>
    /// Gets the authentication type that can be used to determine how this <see cref="ClaimsIdentity"/> authenticated to an authority.
    /// </summary>
    public virtual string? AuthenticationType => ProviderOptions.AuthenticationType;

    public virtual AppIdentityOptions ProviderOptions { protected get; set; } = new();

    /// <summary>
    /// Gets the raw JWT encoded string.
    /// </summary>
    public virtual string? Jwt { get; protected set; }

    public virtual Exception? Error { get; protected set; }

    public virtual bool IsValidated { get; protected set; } = true;

    // Required for de-serialization
    public AppIdentity()
    {
    }

    public AppIdentity(string? jwtEncodedString)
    {
        if (string.IsNullOrEmpty(jwtEncodedString))
            return;
        Jwt = jwtEncodedString;
        try
        {
            _token = new JwtSecurityToken(jwtEncodedString);
            if (_token == null)
                return;
            if (_token.ValidFrom > DateTime.MinValue)
                ValidFrom = _token.ValidFrom;
            if (_token.ValidTo > DateTime.MinValue)
                ValidTo = _token.ValidTo;
            if (HasValidDates)
            {
                _claims.AddRange(_token.Claims);
                _claimsIdentity = new ClaimsIdentity(this, _claims, ProviderOptions.AuthenticationType, ProviderOptions.NameType, ProviderOptions.RoleType);
            }
        }
        catch (Exception exception)
        {
            Error = exception;
        }
    }

    protected void AddClaim(string claimType, string claimValue)
    {
        _claims.Add(new Claim(claimType, claimValue));
    }

    protected void AddClaim(string claimType, DateTime claimValue)
    {
        _claims.Add(new Claim(claimType, claimValue.Subtract(DateTime.UnixEpoch).TotalSeconds.ToString()));
    }
}
