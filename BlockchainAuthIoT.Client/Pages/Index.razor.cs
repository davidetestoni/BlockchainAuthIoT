﻿using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Client.Services;
using BlockchainAuthIoT.Core.Extensions;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Core.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using static Nethereum.Util.UnitConversion;

namespace BlockchainAuthIoT.Client.Pages
{
    public partial class Index
    {
        [Inject] private TestAccountProvider AccountProvider { get; set; }
        [Inject] private AccessControlService AccessControl { get; set; }

        private string contractAddress = string.Empty;
        private string signerAddress = string.Empty;
        private decimal contractPriceEth = 0;
        private string newAdmin = string.Empty;
        private OCPModel newOCP = new();
        private PolicyModel newPolicy = new();
        private ProposalModel newProposal = new();
        private string privateKey = string.Empty;
        private string query = string.Empty;
        private string queryResult = string.Empty;

        protected override void OnInitialized()
        {
            if (AccessControl.ContractLoaded)
            {
                contractAddress = AccessControl.ContractAddress;
            }
        }

        private async Task RefreshBalance()
        {
            await AccountProvider.RefreshBalance();
            await InvokeAsync(StateHasChanged);
        }

        #region Contract
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
                newAdmin = string.Empty;
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

        private async Task InitializeContract()
        {
            try
            {
                await AccessControl.InitializeContract(contractPriceEth);
                await AlertSuccess("Contract initialized");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task SignContract()
        {
            if (await Confirm($"You are about to send {AccessControl.Price.ToEth()} ETH to sign the contract. Is it ok?"))
            {
                try
                {
                    await AccessControl.SignContract();
                    await AlertSuccess("Contract signed");
                }
                catch (Exception ex)
                {
                    await AlertException(ex);
                }
            }
        }

        private async Task CreateOCP()
        {
            try
            {
                await AccessControl.CreateOCP(newOCP);
                await AlertSuccess("Created new OCP");
                newOCP = new();
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task GetOCPBoolParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPBoolParam(ocp, name);
                await AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task GetOCPIntParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPIntParam(ocp, name);
                await AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task GetOCPStringParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = await AccessControl.GetOCPStringParam(ocp, name);
                await AlertSuccess($"The value is: {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task SetOCPBoolParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = bool.Parse(await GetPrompt("Enter the bool parameter value"));
                await AccessControl.SetOCPBoolParam(ocp, name, value);
                await AlertSuccess($"Added bool parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task SetOCPIntParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = int.Parse(await GetPrompt("Enter the int parameter value"));
                await AccessControl.SetOCPIntParam(ocp, name, value);
                await AlertSuccess($"Added int parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task SetOCPStringParam(OCP ocp)
        {
            try
            {
                var name = await GetPrompt("Enter the parameter name");
                var value = await GetPrompt("Enter the string parameter value");
                await AccessControl.SetOCPStringParam(ocp, name, value);
                await AlertSuccess($"Added string parameter {name} = {value}");
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task CreatePolicy()
        {
            try
            {
                await AccessControl.CreatePolicy(newPolicy);
                await AlertSuccess("Created new policy");
                newPolicy = new();
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task CreateProposal()
        {
            try
            {
                await AccessControl.CreateProposal(newProposal);
                await AlertSuccess("Created new proposal");
                newProposal = new();
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }

        private async Task AcceptProposal(Proposal proposal)
        {
            if (await Confirm($"You are about to send {proposal.Price.ToEth()} ETH to accept the proposal. Is it ok?"))
            {
                try
                {
                    await AccessControl.AcceptProposal(proposal);
                    await AlertSuccess("Proposal accepted");
                }
                catch (Exception ex)
                {
                    await AlertException(ex);
                }
            }
        }
        #endregion

        #region Queries
        private async Task QueryResource()
        {
            try
            {
                // Generate the token
                var timestamp = DateTime.UtcNow.ToUnixTime();
                var message = $"{contractAddress}|{AccountProvider.CurrentIdentity}|{timestamp}";
                var signature = new MessageSigner().HashAndSign(message, privateKey);
                var token = $"{message}|{signature}";
                await Log($"Generated token: {token}");

                // Perform the GET request
                using var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Token", token);
                using var response = await client.GetAsync(query);
                queryResult = await response.Content.ReadAsStringAsync();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await AlertException(ex);
            }
        }
        #endregion

        private async Task Log(string message)
            => await js.InvokeVoidAsync("console.log", message);

        private async Task<bool> Confirm(string message)
            => await js.InvokeAsync<bool>("confirm", message);

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

        private ValueTask<string> GetPrompt(string message) =>
            js.InvokeAsync<string>("prompt", message);

        private static decimal ToEth(BigInteger wei)
            => UnitConversion.Convert.FromWei(wei, EthUnit.Ether);
    }
}
