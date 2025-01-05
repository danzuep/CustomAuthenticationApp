namespace CustomAuthenticationApp.Models
{
    /// <summary>
    /// This exception class is thrown by the application if it encounters an unrecoverable error.
    /// </summary>
    [Serializable]
    public class AuthorizationException : Exception
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <overloads>There are four overloads for the constructor</overloads>
        public AuthorizationException()
        {
        }

        /// <inheritdoc />
        public AuthorizationException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public AuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc cref="Exception(string?, Exception?)" />
        public AuthorizationException(Exception innerException) : base(innerException.Message, innerException)
        {
        }
    }
}