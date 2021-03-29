using BlockchainAuthIoT.DataProvider.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlockchainAuthIoT.DataProvider
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<DataEntity> Data { get; set; }
    }
}
