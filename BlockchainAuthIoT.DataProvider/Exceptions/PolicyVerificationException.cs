using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class PolicyVerificationException : Exception
    {
        public PolicyVerificationException()
        {

        }

        public PolicyVerificationException(string resource, string message)
            : base($"Error when verifying the policy for resource '{resource}': {message}")
        {

        }
    }
}
