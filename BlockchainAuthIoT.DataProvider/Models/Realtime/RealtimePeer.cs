using LiteNetLib;

namespace BlockchainAuthIoT.DataProvider.Models.Realtime
{
    public class RealtimePeer
    {
        public RealtimePeer(NetPeer netPeer, string contractAddress, string resource)
        {
            NetPeer = netPeer;
            ContractAddress = contractAddress;
            Resource = resource;
        }

        public NetPeer NetPeer { get; set; }
        public string ContractAddress { get; set; }
        public string Resource { get; set; }
    }
}
