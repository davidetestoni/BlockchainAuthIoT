using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface IPolicyVerificationService
    {
        Task VerifyPolicy(string contractAddress, string resource, List<PolicyRule> rules);
    }
}
