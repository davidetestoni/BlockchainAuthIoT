using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Services
{
    public interface IAccountProvider
    {
        bool AccountLoaded { get; }
        string Address { get; }
        Account Account { get; }
        BigInteger Balance { get; }
        decimal BalanceEth { get; }

        Task<Account> CreateAccount(string keystoreFile, string password);
        Task<Account> LoadAccount(string keystoreFile, string password);
        Task<BigInteger> RefreshBalance();
    }
}
