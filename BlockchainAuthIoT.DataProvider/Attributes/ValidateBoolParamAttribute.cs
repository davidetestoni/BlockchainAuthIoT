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

        public ValidateBoolParamAttribute(string queryParamName, BoolCondition condition, string policyParamName)
        {
            this.queryParamName = queryParamName;
            this.condition = condition;
            this.policyParamName = policyParamName;
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
