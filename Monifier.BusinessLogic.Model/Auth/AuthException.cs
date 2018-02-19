using System;

namespace Monifier.BusinessLogic.Model.Auth
{
    public class AuthException : ApplicationException
    {
        public AuthException(string message) : base(message)
        {
        }

        public AuthException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}