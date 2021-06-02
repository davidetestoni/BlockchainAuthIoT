using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Utils;
using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Extensions;
using BlockchainAuthIoT.Shared.Services;
using Microsoft.Extensions.Caching.Distributed;
using Nethereum.Signer;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class TokenVerificationService : ITokenVerificationService
    {
        private readonly IDistributedCache _cache;
        private readonly IWeb3Provider _web3Provider;

        public TimeSpan TokenValidity { get; set; } = TimeSpan.FromHours(1);

        public TokenVerificationService(IDistributedCache cache, IWeb3Provider web3Provider)
        {
            _cache = cache;
            _web3Provider = web3Provider;
        }

        /// <inheritdoc/>
        public async Task<string> VerifyToken(string token)
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
                throw new TokenVerificationException("Signature mismatch");
            }

            // Verify that the message signature hasn't expired
            var timestamp = TimeConverter.ToDateTimeUtc(BigInteger.Parse(providedTimestamp));
            
            if ((DateTime.UtcNow - timestamp) > TokenValidity)
            {
                throw new TokenVerificationException("The signature has expired");
            }

            // Verify that the signer is registered in the contract
            // Try to get it from the cache
            var signer = await _cache.GetRecordAsync<string>(providedContractAddress);

            // On cache miss, query the contract and update the cache
            if (signer is null)
            {
                AccessControl ac;

                try
                {
                    ac = await AccessControl.FromChain(_web3Provider.Web3, providedContractAddress);
                }
                catch
                {
                    throw new ContractNotFoundException(providedContractAddress);
                }

                // Make sure the contract is signed
                var isSigned = await ac.IsSigned();

                if (!isSigned)
                {
                    throw new InvalidContractException(providedContractAddress, "The contract is not signed");
                }

                signer = await ac.GetSigner();
                await _cache.SetRecordAsync(providedContractAddress, signer, TokenValidity, TokenValidity);
            }

            if (!providedSignerAddress.Equals(signer, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidContractException(providedContractAddress, "The user is not the signer of the provided contract");
            }

            return providedContractAddress;
        }
    }
}
