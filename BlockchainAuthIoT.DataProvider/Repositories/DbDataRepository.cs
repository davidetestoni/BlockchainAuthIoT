using BlockchainAuthIoT.DataProvider.Entities;
using System.Linq;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public class DbDataRepository : IDataRepository
    {
        private readonly AppDbContext context;

        public DbDataRepository(AppDbContext context)
        {
            this.context = context;
        }

        public IQueryable<DataEntity> GetAll()
            => context.Data;
    }
}
