using System;

namespace BlockchainAuthIoT.DataProvider.Entities
{
    public class DataEntity
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Device { get; set; }
        public byte[] Data { get; set; }
    }
}
