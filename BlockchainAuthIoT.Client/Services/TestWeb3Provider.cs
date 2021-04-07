using Nethereum.Web3;

namespace BlockchainAuthIoT.Client.Services
{
    public class TestWeb3Provider : IWeb3Provider
    {
        public Web3 Web3 { get; init; }

        public TestWeb3Provider(string connectionString)
        {
            // Initialize web3 using a local testchain
            Web3 = new Web3(connectionString);
        }
    }
}
