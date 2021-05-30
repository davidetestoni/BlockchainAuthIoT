using BlockchainAuthIoT.Shared.Services;
using Nethereum.KeyStore;
using Nethereum.KeyStore.Model;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using static Nethereum.Util.UnitConversion;

namespace BlockchainAuthIoT.Client.Services
{
    public class AccountProvider : IAccountProvider
    {
        private BigInteger balance = 0;
        private Account account = null;
        private readonly IWeb3Provider _web3Provider;

        public bool AccountLoaded => account is not null;
        public string Address => account?.Address.ToLower();
        public Account Account => account;
        public BigInteger Balance => balance;
        public decimal BalanceEth => UnitConversion.Convert.FromWei(balance, EthUnit.Ether);

        public AccountProvider(IWeb3Provider web3Provider)
        {
            _web3Provider = web3Provider;
        }

        public async Task<Account> CreateAccount(string keystoreFile, string password)
        {
            var keyStoreService = new KeyStoreScryptService();
            var scryptParams = new ScryptParams { Dklen = 32, N = 262144, R = 8, P = 1 };
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, ecKey.GetPrivateKeyAsBytes(),
                ecKey.GetPublicAddress(), scryptParams);
            var json = keyStoreService.SerializeKeyStoreToJson(keyStore);

            File.WriteAllText(Path.Combine("Keystore", keystoreFile), json);
            account = Account.LoadFromKeyStore(json, password);
            await RefreshBalance();
            return account;
        }

        public async Task<Account> LoadAccount(string keystoreFile, string password)
        {
            var json = File.ReadAllText(Path.Combine("Keystore", keystoreFile));
            account = Account.LoadFromKeyStore(json, password);
            await RefreshBalance();
            return account;
        }

        public async Task<BigInteger> RefreshBalance()
        {
            if (account is null)
            {
                throw new Exception("Load an account first");
            }

            balance = await _web3Provider.Web3.Eth.GetBalance.SendRequestAsync(Address);
            return balance;
        }
    }
}
