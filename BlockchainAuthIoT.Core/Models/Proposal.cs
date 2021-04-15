using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class Proposal
    {
        public uint Id { get; set; } = 0;

        [Parameter("bool", 1)]
        public bool Accepted { get; set; }

        [Parameter("uint", 2)]
        public BigInteger Price { get; set; }

        [Parameter("uint", 3)]
        public BigInteger AmountPaid { get; set; }

        [Parameter("bytes32", 4)]
        public byte[] HashCode { get; set; }

        [Parameter("string", 5)]
        public string ExternalResource { get; set; }
    }
}
