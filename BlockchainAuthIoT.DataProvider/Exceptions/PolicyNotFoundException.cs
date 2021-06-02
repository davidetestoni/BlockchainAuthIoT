using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class PolicyNotFoundException : Exception
    {
        public string Resource { get; set; }

        public PolicyNotFoundException(string resource)
            : base($"No off-chain or on-chain policy found for resource {resource}")
        {
            Resource = resource;
        }
    }
}
