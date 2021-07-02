using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Exceptions.Api;
using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using BlockchainAuthIoT.DataProvider.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Attributes
{
    public class PolicyParamFilterAttribute : ActionFilterAttribute
    {
        // TODO: We only need to verify the policy once, and then the rules separately.
        // Calling the whole policy verification for each rule is inefficient, even if cached!
        protected async Task VerifyPolicyRule(ActionExecutingContext context, ActionExecutionDelegate next, PolicyRule rule)
        {
            var policyVerification = (IPolicyVerificationService)context.HttpContext
                .RequestServices.GetService(typeof(IPolicyVerificationService));

            var contractAddress = (string)context.HttpContext.Items["contractAddress"];
            var resourceName = context.HttpContext.Request.Path.Value.TrimStart('/');

            try
            {
                await policyVerification.VerifyPolicy(contractAddress, resourceName, rule);
                await next();
            }
            catch (PolicyVerificationException ex)
            {
                throw new ForbiddenException($"Policy verification failed: {ex.Message}", ex);
            }
            catch (PolicyRuleVerificationException ex)
            {
                throw new ForbiddenException($"Policy verification failed: {ex.Message}", ex);
            }
            catch (ContractNotFoundException ex)
            {
                throw new NotFoundException($"Contract not found at {ex.Address}", ex);
            }
            catch (PolicyNotFoundException ex)
            {
                throw new NotFoundException($"The policy for resource {ex.Resource} was not found", ex);
            }
        }
    }
}
