using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Helpers;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Queries
    {
        [Inject] private IAccountProvider AccountProvider { get; set; }
        [Inject] private AccessControlService AccessControl { get; set; }

        private string query = string.Empty;
        private string queryResult = string.Empty;

        private async Task QueryResource()
        {
            try
            {
                // Generate the token, using the ethereum address of the current user as the public key
                var token = TokenGenerator.Generate(AccessControl.ContractAddress,
                    AccountProvider.Address, AccountProvider.Account.PrivateKey);
                await js.Log($"Generated token: {token}");

                // Perform the GET request
                using var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Token", token);
                using var response = await client.GetAsync(query);
                queryResult = await response.Content.ReadAsStringAsync();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
