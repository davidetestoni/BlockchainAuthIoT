using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class TokenVerificationException : Exception
    {
        public TokenVerificationException()
        {

        }

        public TokenVerificationException(string message) : base(message)
        {

        }

        public TokenVerificationException(string message, Exception inner): base(message, inner)
        {

        }
    }
}
