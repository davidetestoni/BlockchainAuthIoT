using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ContractDeployer
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public EventCallback<string> OnDeployed { get; set; }

        private string signerAddress = string.Empty;

        private async Task DeployNewContract()
        {
            try
            {
                await AccessControl.DeployNewContract(signerAddress);
                var contractAddress = AccessControl.ContractAddress;
                await js.AlertSuccess($"Deployed new contract at {contractAddress}");
                await OnDeployed.InvokeAsync(contractAddress);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
