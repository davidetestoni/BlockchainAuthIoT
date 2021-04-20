using System;

namespace BlockchainAuthIoT.Client.Models
{
    public class ProposalModel
    {
        public string Resource { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0; // In ETH
    }
}
