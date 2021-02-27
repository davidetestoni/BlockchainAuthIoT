using System;

namespace BlockchainAuthIoT.Client.Models
{
    public class OCPModel
    {
        public string Resource { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);
    }
}
