using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface ITokenVerificationService
    {
        /// <summary>
        /// Verifies that a <paramref name="token"/> is valid and that the user is the signer of the contract.
        /// Returns the address of the contract for Access Control purposes.
        /// </summary>
        Task<string> VerifyToken(string token);
    }
}
