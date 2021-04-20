using System.Threading.Tasks;

namespace BlockchainAuthIoT.Shared.Repositories
{
    public interface IPolicyDatabase
    {
        Task<byte[]> GetPolicy(string location);
    }
}
