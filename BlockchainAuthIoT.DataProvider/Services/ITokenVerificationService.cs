using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface ITokenVerificationService
    {
        Task VerifyToken(string token);
    }
}
