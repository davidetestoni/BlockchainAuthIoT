using BlockchainAuthIoT.DataProvider.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public interface IHumidityRepository
    {
        IQueryable<HumidityEntity> GetAll();
        Task<List<HumidityEntity>> GetLatest(int count, string[] deviceNames);
    }
}
