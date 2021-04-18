using System;

namespace BlockchainAuthIoT.Models
{
    public abstract class Reading
    {
        public DateTime Date { get; set; }
        public string Device { get; set; }
    }
}
