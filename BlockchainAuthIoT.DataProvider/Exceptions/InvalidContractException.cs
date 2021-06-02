using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class InvalidContractException : Exception
    {
        public string Address { get; set; }

        public InvalidContractException(string address, string message)
            : base($"Invalid contract at {address}: {message}")
        {
            Address = address;
        }
    }
}
