using BlockchainAuthIoT.Core.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Numerics;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class Proposal
    {
        public uint Id { get; set; } = 0;
        public DateTime StartTime => StartTimeUnix.ToDateTimeUtc();
        public TimeSpan Duration => TimeSpan.FromSeconds((double)DurationUnix);

        [Parameter("bool", 1)]
        public bool Finalized { get; set; }

        [Parameter("bool", 2)]
        public bool Approved { get; set; }

        [Parameter("string", 3)]
        public string Resource { get; set; }

        [Parameter("uint256", 4)]
        public BigInteger StartTimeUnix { get; set; }

        [Parameter("uint256", 5)]
        public BigInteger DurationUnix { get; set; }
    }
}
