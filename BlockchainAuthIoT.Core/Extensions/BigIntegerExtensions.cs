using Nethereum.Util;
using System.Numerics;

namespace BlockchainAuthIoT.Core.Extensions
{
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Helper method to convert from wei to ether.
        /// </summary>
        public static decimal ToEth(this BigInteger wei)
            => UnitConversion.Convert.FromWei(wei, UnitConversion.EthUnit.Ether);
    }
}
