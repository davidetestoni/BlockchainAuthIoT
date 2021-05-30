using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class AdminManager
    {
        [Inject] private AccessControlService AccessControl { get; set; }

        private string newAdmin = string.Empty;

        private async Task AddAdmin()
        {
            try
            {
                await AccessControl.AddAdmin(newAdmin);
                await js.AlertSuccess($"Admin {newAdmin} added");
                newAdmin = string.Empty;
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task RemoveAdmin(string admin)
        {
            try
            {
                await AccessControl.RemoveAdmin(admin);
                await js.AlertSuccess($"Admin {admin} removed");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
