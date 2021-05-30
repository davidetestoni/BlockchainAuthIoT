using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class PolicyManager
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public bool IsAdmin { get; set; }

        private PolicyModel newPolicy = new();

        private async Task CreatePolicy()
        {
            try
            {
                await AccessControl.CreatePolicy(newPolicy);
                await js.AlertSuccess("Created new policy");
                newPolicy = new();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
