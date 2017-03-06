using System;

namespace AuthTokenServer.Exceptions
{
    public class ExternalAuthException : AuthException
    {
        public string Description { get; protected set; }
        public ExternalAuthException(string message, string description = null) : base(message)
        {
            Description = description;
        }
    }
}