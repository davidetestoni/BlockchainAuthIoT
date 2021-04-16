using System;

namespace BlockchainAuthIoT.Client.Models
{
    public class ProposalModel
    {
        public string ExternalResource { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0; // In ETH
    }
}
