using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface IUserVerificationService
    {
        Task VerifyToken(string token);
    }
}
