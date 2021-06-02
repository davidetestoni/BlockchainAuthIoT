using System;

namespace BlockchainAuthIoT.DataProvider.Exceptions
{
    public class PolicyRuleVerificationException : Exception
    {
        public string Resource { get; set; }
        public string Parameter { get; set; }

        public PolicyRuleVerificationException(string resource, string parameter)
            : base($"Rule on parameter '{parameter}' was not respected")
        {
            Resource = resource;
            Parameter = parameter;
        }
    }
}
