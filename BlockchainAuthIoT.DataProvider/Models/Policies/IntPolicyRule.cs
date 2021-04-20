using System;

namespace BlockchainAuthIoT.DataProvider.Models.Policies
{
    public class IntPolicyRule : PolicyRule
    {
        public Func<int, bool> Function { get; set; }

        public IntPolicyRule(string name, Func<int, bool> function)
        {
            Parameter = name;
            Function = function;
        }
    }
}
