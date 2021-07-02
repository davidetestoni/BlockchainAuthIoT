using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Attributes
{
    public class ValidateStringParamAttribute : PolicyParamFilterAttribute
    {
        private readonly string queryParamName;
        private readonly StringCondition condition;
        private readonly string policyParamName;

        public ValidateStringParamAttribute(string queryParamName, StringCondition condition, string policyParamName = null)
        {
            this.queryParamName = queryParamName;
            this.condition = condition;
            this.policyParamName = policyParamName ?? queryParamName;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var providedValue = context.HttpContext.Request.Query[queryParamName];
            var rule = new StringPolicyRule(policyParamName, value =>
            {
                return condition switch
                {
                    StringCondition.Contains => providedValue.Contains(value),
                    StringCondition.IsContainedIn => value.Contains(providedValue),
                    StringCondition.DoesNotContain => !providedValue.Contains(value),
                    StringCondition.IsNotContainedIn => !value.Contains(providedValue),
                    _ => throw new NotImplementedException()
                };
            });

            await VerifyPolicyRule(context, next, rule);
        }
    }

    public enum StringCondition
    {
        Contains,
        IsContainedIn,
        DoesNotContain,
        IsNotContainedIn
    }
}
