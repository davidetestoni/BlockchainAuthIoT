using BlockchainAuthIoT.DataProvider.Entities;
using System.Linq;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public interface IDataRepository
    {
        IQueryable<DataEntity> GetAll();
    }
}
