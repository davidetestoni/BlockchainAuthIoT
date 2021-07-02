using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions.Api
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message, Exception inner = null) : base(message, inner) { }
    }
}
