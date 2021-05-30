using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Services;
using BlockchainAuthIoT.Core.Utils;
using Microsoft.AspNetCore.Components;
using Nethereum.Signer;
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
                // Generate the token
                var timestamp = DateTime.UtcNow.ToUnixTime();
                var message = $"{AccessControl.ContractAddress}|{AccountProvider.Address}|{timestamp}";
                var signature = new MessageSigner().HashAndSign(message, AccountProvider.Account.PrivateKey);
                var token = $"{message}|{signature}";
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
