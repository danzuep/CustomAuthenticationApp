namespace CustomAuthenticationApp.Models
{
    //public record struct LoginCredential(string User, string Password);
    public record LoginCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public LoginCredential() : this(string.Empty, string.Empty)
        {
        }

        public LoginCredential(string user, string password)
        {
            Username = user;
            Password = password;
        }
    }
}
