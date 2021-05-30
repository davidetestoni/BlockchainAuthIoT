using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Wallet
    {
        [Inject] private IAccountProvider AccountProvider { get; set; }

        private readonly WalletModel wallet = new()
        {
            KeystoreFile = "wallet.json",
            Password = string.Empty
        };

        private async Task Create()
        {
            try
            {
                await AccountProvider.CreateAccount(wallet.KeystoreFile, wallet.Password);
                await js.AlertSuccess($"Created {AccountProvider.Address}");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task Load()
        {
            try
            {
                await AccountProvider.LoadAccount(wallet.KeystoreFile, wallet.Password);
                await js.AlertSuccess($"Loaded {AccountProvider.Address}");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task RefreshBalance()
        {
            try
            {
                await AccountProvider.RefreshBalance();
                await js.AlertSuccess($"The balance is {AccountProvider.BalanceEth} ETH");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
