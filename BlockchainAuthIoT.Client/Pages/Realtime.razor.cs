using BlockchainAuthIoT.Client.Helpers;
using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Components;
using System;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Realtime : IDisposable
    {
        [Inject] private IAccountProvider AccountProvider { get; set; }
        [Inject] private AccessControlService AccessControl { get; set; }
        [Inject] private RealtimeClient RealtimeClient { get; set; }

        private readonly RemoteModel remote = new();
        private readonly FixedSizedQueue<string> buffer = new(20);

        private void Connect()
        {
            var token = TokenGenerator.Generate(AccessControl.ContractAddress, 
                AccountProvider.Address, AccountProvider.Account.PrivateKey);

            RealtimeClient.MessageReceived += UpdateBuffer;

            RealtimeClient.Connect(remote.Host, remote.Port, remote.Resource, token);
            StateHasChanged();
        }

        private void Disconnect()
        {
            RealtimeClient.MessageReceived -= UpdateBuffer;
            RealtimeClient.Disconnect();
            StateHasChanged();
        }

        private async void UpdateBuffer(object sender, string message)
        {
            buffer.Enqueue(message);
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            RealtimeClient.MessageReceived -= UpdateBuffer;
            RealtimeClient.Disconnect();
        }
    }
}
