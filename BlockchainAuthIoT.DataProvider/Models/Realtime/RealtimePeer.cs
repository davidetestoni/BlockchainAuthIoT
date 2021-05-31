using LiteNetLib;

namespace BlockchainAuthIoT.DataProvider.Models.Realtime
{
    public class RealtimePeer
    {
        public RealtimePeer(NetPeer netPeer, string contractAddress)
        {
            NetPeer = netPeer;
            ContractAddress = contractAddress;
        }

        public NetPeer NetPeer { get; set; }
        public string ContractAddress { get; set; }
    }
}
