using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Client.Services
{
    public class AccessControlService
    {
        private readonly IWeb3Provider web3Provider;
        private readonly IAccountProvider accountProvider;
        private AccessControl contract;

        private bool initialized = false;
        private List<string> admins = new();
        private List<OCP> ocps = new();
        private List<Policy> policies = new();
        private List<Proposal> proposals = new();

        public bool ContractLoaded { get; private set; } = false;
        public string ContractAddress => contract.Address;
        public IEnumerable<string> Admins => admins;
        public IEnumerable<OCP> OCPs => ocps;
        public IEnumerable<Policy> Policies => policies;
        public IEnumerable<Proposal> Proposals => proposals;
        public bool Initialized => initialized;

        public AccessControlService(IWeb3Provider web3Provider, IAccountProvider accountProvider)
        {
            this.web3Provider = web3Provider;
            this.accountProvider = accountProvider;
        }

        public async Task DeployNewContract(string signer)
        {
            var contract = await AccessControl.Deploy(web3Provider.Web3, accountProvider.Address, signer);
            await LoadContract(contract.Address);
        }

        public async Task LoadContract(string address)
        {
            contract = await AccessControl.FromChain(web3Provider.Web3, address);
            ContractLoaded = true;

            initialized = await contract.IsInitialized();
            await RefreshAdmins();
            await RefreshOCPs();
            await RefreshPolicies();
            await RefreshProposals();
        }

        public async Task AddAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.AddAdmin(accountProvider.Address, adminAddress);
            await RefreshAdmins();
        }

        public async Task RemoveAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.RemoveAdmin(accountProvider.Address, adminAddress);
            await RefreshAdmins();
        }

        public async Task InitializeContract()
        {
            EnsureLoaded();
            await contract.InitializeContract(accountProvider.Address);
            initialized = await contract.IsInitialized();
        }

        private async Task RefreshAdmins()
            => admins = (await contract.GetAdmins()).ToList();

        private async Task RefreshOCPs()
            => ocps = (await contract.GetOCPs()).ToList();

        private async Task RefreshPolicies()
            => policies = (await contract.GetPolicies()).ToList();

        private async Task RefreshProposals()
            => proposals = (await contract.GetProposals()).ToList();

        private void EnsureLoaded()
        {
            if (contract == null)
                throw new Exception("Load a contract first!");
        }
    }
}
