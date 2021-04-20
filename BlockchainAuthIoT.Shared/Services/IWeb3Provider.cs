using Nethereum.Web3;

namespace BlockchainAuthIoT.Shared.Services
{
    public interface IWeb3Provider
    {
        public Web3 Web3 { get; }
    }
}
