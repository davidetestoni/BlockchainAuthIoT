using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class Proposal
    {
        public uint Id { get; set; } = 0;

        [Parameter("bool", 1)]
        public bool Approved { get; set; }

        [Parameter("bytes32", 2)]
        public byte[] HashCode { get; set; }

        [Parameter("string", 3)]
        public string ExternalResource { get; set; }
    }
}
