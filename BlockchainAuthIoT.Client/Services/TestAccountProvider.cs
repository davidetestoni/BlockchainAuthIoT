namespace BlockchainAuthIoT.Client.Services
{
    public class TestAccountProvider : IAccountProvider
    {
        public string Address { get; init; }

        public TestAccountProvider(string address)
        {
            Address = address;
        }
    }
}
