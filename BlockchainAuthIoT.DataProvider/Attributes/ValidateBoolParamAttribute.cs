using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Attributes
{
    public class ValidateBoolParamAttribute : PolicyParamFilterAttribute
    {
        private readonly string queryParamName;
        private readonly BoolCondition condition;
        private readonly string policyParamName;

        /// <summary>
        /// Adds blockchain policy-based validation for a boolean parameter.
        /// </summary>
        /// <param name="queryParamName">The name of the parameter in the query string</param>
        /// <param name="condition">The condition when comparing it to the policy-enforced rule</param>
        /// <param name="policyParamName">The name of the parameter in the policy</param>
        public ValidateBoolParamAttribute(string queryParamName, BoolCondition condition, string policyParamName = null)
        {
            this.queryParamName = queryParamName;
            this.condition = condition;
            this.policyParamName = policyParamName ?? queryParamName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var providedValue = bool.Parse(context.HttpContext.Request.Query[queryParamName]);
            var rule = new BoolPolicyRule(policyParamName, value => condition == BoolCondition.Equal
                ? value == providedValue 
                : value != providedValue);

            await VerifyPolicyRule(context, next, rule);
        }
    }

    public enum BoolCondition
    {
        Equal,
        NotEqual
    }
}
