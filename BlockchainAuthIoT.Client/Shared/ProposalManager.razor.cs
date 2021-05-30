using BlockchainAuthIoT.Client.Extensions;
using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using BlockchainAuthIoT.Core.Extensions;
using BlockchainAuthIoT.Core.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Shared
{
    public partial class ProposalManager
    {
        [Inject] private AccessControlService AccessControl { get; set; }
        [Parameter] public bool IsAdmin { get; set; }

        private ProposalModel newProposal = new();

        private async Task CreateProposal()
        {
            try
            {
                await AccessControl.CreateProposal(newProposal);
                await js.AlertSuccess("Created new proposal");
                newProposal = new();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task AcceptProposal(Proposal proposal)
        {
            if (await js.Confirm($"You are about to send {proposal.Price.ToEth()} ETH to accept the proposal. Is it ok?"))
            {
                try
                {
                    await AccessControl.AcceptProposal(proposal);
                    await js.AlertSuccess("Proposal accepted");
                }
                catch (Exception ex)
                {
                    await js.AlertException(ex);
                }
            }
        }
    }
}
