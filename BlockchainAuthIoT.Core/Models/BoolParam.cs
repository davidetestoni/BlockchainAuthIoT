using Nethereum.ABI.FunctionEncoding.Attributes;

namespace BlockchainAuthIoT.Core.Models
{
    [FunctionOutput]
    public class BoolParam
    {
        public int Index { get; set; } = 0;

        [Parameter("string", 1)]
        public string Name { get; set; }

        [Parameter("bool", 2)]
        public bool Value { get; set; }
    }
}
