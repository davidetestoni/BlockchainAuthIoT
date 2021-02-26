using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Index
    {
        [Inject] private AccessControlService AccessControl { get; set; }

        private string contractAddress = string.Empty;
        private string signerAddress = string.Empty;
        private string newAdmin = string.Empty;

        protected override void OnInitialized()
        {
            if (AccessControl.ContractLoaded)
            {
                contractAddress = AccessControl.ContractAddress;
            }
        }

        private async Task DeployNewContract()
        {
            try
            {
                await AccessControl.DeployNewContract(signerAddress);
                contractAddress = AccessControl.ContractAddress;
                await AlertSuccess($"Deployed new contract at {contractAddress}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task LoadContract()
        {
            try
            {
                await AccessControl.LoadContract(contractAddress);
                await AlertSuccess("Contract loaded");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task AddAdmin()
        {
            try
            {
                await AccessControl.AddAdmin(newAdmin);
                await AlertSuccess($"Admin {newAdmin} added");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task RemoveAdmin(string admin)
        {
            try
            {
                await AccessControl.RemoveAdmin(admin);
                await AlertSuccess($"Admin {admin} removed");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task AlertException(Exception ex)
        {
            await js.InvokeVoidAsync("console.log", ex.ToString());
            await js.InvokeVoidAsync("alert", $"ERROR: {ex.Message}");
        }

        private async Task AlertSuccess(string message)
        {
            await js.InvokeVoidAsync("console.log", message);
            await js.InvokeVoidAsync("alert", message);
        }
    }
}
