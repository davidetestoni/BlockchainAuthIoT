using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class StringParam
    {
        public int Index { get; set; } = 0;

        [Parameter("string", 1)]
        public string Name { get; set; }

        [Parameter("string", 2)]
        public string Value { get; set; }
    }
}
