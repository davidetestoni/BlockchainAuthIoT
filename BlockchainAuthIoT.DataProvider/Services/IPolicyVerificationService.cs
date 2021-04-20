using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.DataProvider.Models.Policies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface IPolicyVerificationService
    {
        Task VerifyPolicy(string contractAddress, string resource, List<PolicyRule> rules);
    }
}
