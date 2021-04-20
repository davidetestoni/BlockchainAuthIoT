using System;

namespace BlockchainAuthIoT.DataProvider.Models.Policies
{
    public class StringPolicyRule : PolicyRule
    {
        public Func<string, bool> Function { get; set; }

        public StringPolicyRule(string name, Func<string, bool> function)
        {
            Parameter = name;
            Function = function;
        }
    }
}
