using System;

namespace BlockchainAuthIoT.Client.Services
{
    public class TestAccountProvider
    {
        public string[] Identities { get; init; }
        public string CurrentIdentity { get; set; }

        public TestAccountProvider(IWeb3Provider web3Provider)
        {
            // Unlock accounts
            Identities = web3Provider.Web3.Eth.Accounts.SendRequestAsync().Result;
            Console.WriteLine("Unlocked accounts (for debug purposes):");
            foreach (var account in Identities)
            {
                web3Provider.Web3.Personal.UnlockAccount.SendRequestAsync(account, "password", 120).Wait();
                Console.WriteLine(account);
            }

            CurrentIdentity = Identities[0];
        }
    }
}
