using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions.Api
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message, Exception inner = null) : base(message, inner) { }
    }
}
