using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ContractLoader
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public EventCallback OnLoaded { get; set; }

        private string contractAddress = string.Empty;

        private async Task LoadContract()
        {
            try
            {
                await AccessControl.LoadContract(contractAddress);
                await js.AlertSuccess("Contract loaded");
                await OnLoaded.InvokeAsync();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
