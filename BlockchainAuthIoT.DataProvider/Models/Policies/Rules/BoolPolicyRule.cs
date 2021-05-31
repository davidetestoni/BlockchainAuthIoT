using System;

namespace BlockchainAuthIoT.DataProvider.Models.Policies.Rules
{
    public class BoolPolicyRule : PolicyRule
    {
        public Func<bool, bool> Function { get; set; }

        public BoolPolicyRule(string name, Func<bool, bool> function)
        {
            Parameter = name;
            Function = function;
        }
    }
}
