using BlockchainAuthIoT.DataProvider.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public class DbHumidityRepository : IHumidityRepository
    {
        private readonly AppDbContext context;

        public DbHumidityRepository(AppDbContext context)
        {
            this.context = context;
        }

        public Task<List<HumidityEntity>> GetLatest(int count, string[] deviceNames)
            => GetAll().Where(e => deviceNames.Contains(e.Device)).OrderByDescending(e => e.Id).Take(count).ToListAsync();

        public IQueryable<HumidityEntity> GetAll()
            => context.Humidity;
    }
}
