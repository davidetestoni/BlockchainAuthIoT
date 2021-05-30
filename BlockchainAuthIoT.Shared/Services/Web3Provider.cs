using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BlockchainAuthIoT.Shared.Services
{
    public class Web3Provider : IWeb3Provider
    {
        public Web3 Web3 { get; private set; }
        private readonly string connectionString;

        public Web3Provider(string connectionString)
        {
            this.connectionString = connectionString;
            Web3 = new Web3(connectionString);
        }

        public void Authenticate(Account account)
        {
            Web3 = new Web3(account, connectionString);
        }
    }
}
