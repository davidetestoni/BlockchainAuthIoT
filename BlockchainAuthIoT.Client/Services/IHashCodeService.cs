using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Services
{
    public interface IHashCodeService
    {
        Task<byte[]> ComputeHashCode(string externalResource);
    }
}
