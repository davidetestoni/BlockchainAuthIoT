using Newtonsoft.Json.Linq;
using System;

namespace BlockchainAuthIoT.Core.Exceptions
{
    public class ContractException : Exception
    {
        public JArray Logs { get; set; }

        public ContractException(JArray logs) : base(logs.ToString())
        {
            Logs = logs;
        }
    }
}
