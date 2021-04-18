using System;

namespace BlockchainAuthIoT.DataProvider.Entities
{
    public class TemperatureEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Device { get; set; }
        public double Value { get; set; }
    }
}
