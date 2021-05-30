using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ContractInitializer
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public EventCallback Initialized { get; set; }

        private decimal contractPriceEth = 0;

        private async Task InitializeContract()
        {
            try
            {
                await AccessControl.InitializeContract(contractPriceEth);
                await js.AlertSuccess("Contract initialized");
                await Initialized.InvokeAsync();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
