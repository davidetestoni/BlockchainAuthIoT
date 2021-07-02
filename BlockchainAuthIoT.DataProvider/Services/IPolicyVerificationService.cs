using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public interface IPolicyVerificationService
    {
        // TEMPORARY! Need to separate policy validation and rules validation!
        Task VerifyPolicy(string contractAddress, string resource, PolicyRule rule);
        Task VerifyPolicy(string contractAddress, string resource, List<PolicyRule> rules);
    }
}
