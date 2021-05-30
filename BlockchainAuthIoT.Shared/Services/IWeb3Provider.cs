using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BlockchainAuthIoT.Shared.Services
{
    public interface IWeb3Provider
    {
        Web3 Web3 { get; }

        void Authenticate(Account account);
    }
}
