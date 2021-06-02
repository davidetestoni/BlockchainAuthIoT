using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class ContractNotFoundException : Exception
    {
        public string Address { get; set; }

        public ContractNotFoundException(string address)
            : base($"There is no valid AccessControl contract at {address}")
        {
            Address = address;
        }
    }
}
