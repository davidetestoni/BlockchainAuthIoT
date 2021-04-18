using BlockchainAuthIoT.DataProvider.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public interface ITemperatureRepository
    {
        IQueryable<TemperatureEntity> GetAll();
        Task<List<TemperatureEntity>> GetLatest(int count, string[] deviceNames);
    }
}
