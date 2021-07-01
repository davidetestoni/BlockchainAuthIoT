using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using BlockchainAuthIoT.Core.Extensions;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ContractSigner
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public EventCallback Signed { get; set; }

        private string userPubKey = string.Empty;

        private async Task SignContract()
        {
            if (await js.Confirm($"You are about to send {AccessControl.Price.ToEth()} ETH to sign the contract. Is it ok?"))
            {
                try
                {
                    await AccessControl.SignContract(userPubKey);
                    await js.AlertSuccess("Contract signed");
                    await InvokeAsync(StateHasChanged);
                    await Signed.InvokeAsync();
                }
                catch (Exception ex)
                {
                    await js.AlertException(ex);
                }
            }
        }
    }
}
