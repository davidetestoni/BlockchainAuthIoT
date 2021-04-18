using BlockchainAuthIoT.DataProvider.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Repositories
{
    public class DbTemperatureRepository : ITemperatureRepository
    {
        private readonly AppDbContext context;

        public DbTemperatureRepository(AppDbContext context)
        {
            this.context = context;
        }

        public Task<List<TemperatureEntity>> GetLatest(int count, string[] deviceNames)
            => GetAll().Where(e => deviceNames.Contains(e.Device)).OrderByDescending(e => e.Id).Take(count).ToListAsync();

        public IQueryable<TemperatureEntity> GetAll()
            => context.Temperature;
    }
}
