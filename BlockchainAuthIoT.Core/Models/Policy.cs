using BlockchainAuthIoT.Core.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Numerics;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class Policy
    {
        public uint Id { get; set; } = 0;
        public DateTime StartTime => StartTimeUnix.ToDateTimeUtc();
        public DateTime Expiration => ExpirationUnix.ToDateTimeUtc();

        [Parameter("string", 1)]
        public string Resource { get; set; }

        [Parameter("uint256", 2)]
        public BigInteger StartTimeUnix { get; set; }

        [Parameter("uint256", 3)]
        public BigInteger ExpirationUnix { get; set; }
    }
}
