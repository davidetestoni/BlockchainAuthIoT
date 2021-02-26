using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class IntParam
    {
        public int Index { get; set; } = 0;

        [Parameter("string", 1)]
        public string Name { get; set; }

        [Parameter("int", 2)]
        public int Value { get; set; }
    }
}
