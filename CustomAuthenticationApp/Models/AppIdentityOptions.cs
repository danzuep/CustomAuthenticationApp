using Microsoft.IdentityModel.Tokens;

namespace CustomAuthenticationApp.Models
{
    public class AppIdentityOptions
    {
        public TokenValidationParameters TokenValidationParameters { get; set; } = new();
        public string? AuthenticationType { get; set; } = _defaultAuthenticationType;
        public string? NameType { get; set; } = _defaultNameType; // e.g. JwtRegisteredClaimNames.Sub;
        public string? RoleType { get; set; } = _defaultRoleType;

        internal static readonly string _defaultAuthenticationType = "AppIdentity";
        internal static readonly string _defaultNameType = "name";
        internal static readonly string _defaultRoleType = "role";
    }
}
