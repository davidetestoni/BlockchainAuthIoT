namespace BlockchainAuthIoT.Client.Services
{
    // This is just for testing on the testchain with unlocked accounts
    // TODO: Convert this to actual production code
    public interface IAccountProvider
    {
        public string Address { get; }
    }
}
