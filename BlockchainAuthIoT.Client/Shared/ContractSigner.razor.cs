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

        private async Task SignContract()
        {
            if (await js.Confirm($"You are about to send {AccessControl.Price.ToEth()} ETH to sign the contract. Is it ok?"))
            {
                try
                {
                    await AccessControl.SignContract();
                    await js.AlertSuccess("Contract signed");
                }
                catch (Exception ex)
                {
                    await js.AlertException(ex);
                }
            }
        }
    }
}
