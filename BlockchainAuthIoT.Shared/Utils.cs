using Org.BouncyCastle.Crypto.Digests;

namespace BlockchainAuthIoT.Shared
{
    public static class Utils
    {
        public static byte[] ComputeHashCode(byte[] body)
        {
            // Keccak digest (to match the one from solidity)
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(body, 0, body.Length);
            var calculatedHash = new byte[32];
            digest.DoFinal(calculatedHash, 0);

            return calculatedHash;
        }
    }
}
