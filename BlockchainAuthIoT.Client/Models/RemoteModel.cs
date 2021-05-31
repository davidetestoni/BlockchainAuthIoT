namespace BlockchainAuthIoT.Client.Models
{
    public class RemoteModel
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 6390;
        public string Resource { get; set; } = string.Empty;
    }
}
