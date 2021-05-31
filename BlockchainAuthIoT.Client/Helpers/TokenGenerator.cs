using BlockchainAuthIoT.Core.Utils;
using Nethereum.Signer;
using System;

namespace BlockchainAuthIoT.Client.Helpers
{
    public static class TokenGenerator
    {
        public static string Generate(string contractAddress, string signerAddress, string privateKey)
        {
            var timestamp = DateTime.UtcNow.ToUnixTime();
            var message = $"{contractAddress}|{signerAddress}|{timestamp}";
            var signature = new MessageSigner().HashAndSign(message, privateKey);
            return $"{message}|{signature}";
        }
    }
}
