using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions.Api
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message, Exception inner = null) : base(message, inner) { }
    }
}
