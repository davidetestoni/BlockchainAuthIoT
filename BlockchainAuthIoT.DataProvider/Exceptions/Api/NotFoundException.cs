using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions.Api
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message, Exception inner = null) : base(message, inner) { }
    }
}
