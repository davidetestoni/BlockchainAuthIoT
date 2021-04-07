using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Utils;
using BlockchainAuthIoT.DataProvider.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Nethereum.Signer;
using Nethereum.Web3;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class TokenVerificationService : ITokenVerificationService
    {
        private readonly IDistributedCache _cache;
        private readonly IWeb3Provider _web3Provider;

        public TimeSpan TimestampValidity { get; set; } = TimeSpan.FromHours(1);

        public TokenVerificationService(IDistributedCache cache, IWeb3Provider web3Provider)
        {
            _cache = cache;
            _web3Provider = web3Provider;
        }

        public async Task VerifyToken(string token)
        {
            var split = token.Split('|', 4);
            var providedContractAddress = split[0];
            var providedSignerAddress = split[1];
            var providedTimestamp = split[2];
            var providedSignature = split[3];

            // Verify that the signature is valid
            var signerAddress = new MessageSigner().HashAndEcRecover(
                 $"{providedContractAddress}|{providedSignerAddress}|{providedTimestamp}", providedSignature);

            if (!signerAddress.Equals(providedSignerAddress, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Signature mismatch");
            }

            // Verify that the message signature hasn't expired
            var timestamp = TimeConverter.ToDateTimeUtc(BigInteger.Parse(providedTimestamp));
            
            if ((DateTime.UtcNow - timestamp) > TimestampValidity)
            {
                throw new Exception("The signature has expired");
            }

            // Verify that the signer is registered in the contract
            // Try to get it from the cache
            var signer = await _cache.GetRecordAsync<string>(providedContractAddress);

            // On cache miss, query the contract and update the cache
            if (signer is null)
            {
                var ac = await AccessControl.FromChain(_web3Provider.Web3, providedContractAddress);
                signer = await ac.GetSigner();
                await _cache.SetRecordAsync(providedContractAddress, signer, TimestampValidity, TimestampValidity);
            }

            if (!providedSignerAddress.Equals(signer, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("The user is not the signer of the provided contract");
            }
        }
    }
}
