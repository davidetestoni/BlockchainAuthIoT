using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Services
{
    public class RemoteHashCodeService : IHashCodeService, IDisposable
    {
        private readonly HttpClient httpClient;

        public RemoteHashCodeService()
        {
            httpClient = new();
        }

        public async Task<byte[]> ComputeHashCode(string externalResource)
        {
            var response = await httpClient.GetAsync(externalResource);
            var body = await response.Content.ReadAsByteArrayAsync();

            // Keccak digest (to match the one from solidity)
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(body, 0, body.Length);
            var calculatedHash = new byte[32];
            digest.DoFinal(calculatedHash, 0);

            return calculatedHash;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
