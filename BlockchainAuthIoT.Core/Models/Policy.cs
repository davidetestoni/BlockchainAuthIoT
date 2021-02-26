using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class Policy
    {
        public uint Id { get; set; } = 0;

        [Parameter("bytes32", 1)]
        public byte[] HashCode { get; set; }

        [Parameter("string", 2)]
        public string ExternalResource { get; set; }
    }
}
