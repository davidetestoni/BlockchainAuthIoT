using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Utils;
using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Extensions;
using BlockchainAuthIoT.Shared.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;

        public TimeSpan TokenValidity { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan CacheValidity { get; set; } = TimeSpan.FromHours(1);

        public TokenVerificationService(IDistributedCache cache, IWeb3Provider web3Provider,
            IConfiguration config)
        {
            _cache = cache;
            _web3Provider = web3Provider;
            _config = config;
            TokenValidity = TimeSpan.FromSeconds(int.Parse(config.GetSection("Security")["TokenValidity"]));
            CacheValidity = TimeSpan.FromSeconds(int.Parse(config.GetSection("Caching")["Expiration"]));
        }

        /// <inheritdoc/>
        public async Task<string> VerifyToken(string token)
        {
            var split = token.Split('|', 4);
            var providedContractAddress = split[0];
            var providedUserPubKey = split[1];
            var providedTimestamp = split[2];
            var providedSignature = split[3];

            // Verify that the signature is valid. By default we expect an ethereum address.
            // NOTE: This can be changed if a different encryption mechanism is used in the implementation.
            var userPubKey = new MessageSigner().HashAndEcRecover(
                 $"{providedContractAddress}|{providedUserPubKey}|{providedTimestamp}", providedSignature);

            if (!userPubKey.Equals(providedUserPubKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new TokenVerificationException("Signature mismatch");
            }

            // Verify that the message signature hasn't expired
            var timestamp = TimeConverter.ToDateTimeUtc(BigInteger.Parse(providedTimestamp));
            
            if ((DateTime.UtcNow - timestamp) > TokenValidity)
            {
                throw new TokenVerificationException("The signature has expired");
            }

            // Verify that the user's public key is registered in the contract
            // Try to get it from the cache
            var pubKey = await _cache.GetRecordAsync<string>(providedContractAddress);

            // On cache miss, query the contract and update the cache
            if (pubKey is null)
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

                var authorizedOwner = _config.GetSection("Security")["AuthorizedOwner"];
                var contractOwner = await ac.GetOwner();

                if (!authorizedOwner.Equals(contractOwner, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidContractException(providedContractAddress, "Unauthorized contract owner");
                }

                pubKey = await ac.GetUserPubKey();
                await _cache.SetRecordAsync(providedContractAddress, pubKey, TokenValidity, TokenValidity);
            }

            if (!providedUserPubKey.Equals(pubKey, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidContractException(providedContractAddress, "The provided public key is different from the one in the provided contract");
            }

            return providedContractAddress;
        }
    }
}
