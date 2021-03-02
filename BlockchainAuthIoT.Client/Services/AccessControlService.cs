using BlockchainAuthIoT.Client.Models;
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
        private readonly string emptyAddress = "0x0000000000000000000000000000000000000000";
        private readonly IWeb3Provider web3Provider;
        private readonly TestAccountProvider accountProvider;
        private readonly IHashCodeService hashCodeService;
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

        public AccessControlService(IWeb3Provider web3Provider, TestAccountProvider accountProvider,
            IHashCodeService hashCodeService)
        {
            this.web3Provider = web3Provider;
            this.accountProvider = accountProvider;
            this.hashCodeService = hashCodeService;
        }

        public async Task DeployNewContract(string signer)
        {
            var contract = await AccessControl.Deploy(web3Provider.Web3, accountProvider.CurrentIdentity, signer);
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

        #region Admins and Contract Management
        public async Task AddAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.AddAdmin(accountProvider.CurrentIdentity, adminAddress);
            await RefreshAdmins();
        }

        public async Task RemoveAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.RemoveAdmin(accountProvider.CurrentIdentity, adminAddress);
            await RefreshAdmins();
        }

        public async Task InitializeContract()
        {
            EnsureLoaded();
            await contract.InitializeContract(accountProvider.CurrentIdentity);
            initialized = await contract.IsInitialized();
        }
        #endregion

        #region OCPs (On-Chain Policies) Management
        public async Task CreateOCP(OCPModel model)
        {
            EnsureLoaded();
            await contract.CreateOCP(accountProvider.CurrentIdentity, model.Resource, model.StartTime, model.Expiration);
            await RefreshOCPs();
        }

        public Task<bool> GetOCPBoolParam(OCP ocp, string name)
        {
            EnsureLoaded();
            return contract.GetOCPBoolParam(ocp, name);
        }

        public Task<int> GetOCPIntParam(OCP ocp, string name)
        {
            EnsureLoaded();
            return contract.GetOCPIntParam(ocp, name);
        }

        public Task<string> GetOCPStringParam(OCP ocp, string name)
        {
            EnsureLoaded();
            return contract.GetOCPStringParam(ocp, name);
        }

        public Task SetOCPBoolParam(OCP ocp, string name, bool value)
        {
            EnsureLoaded();
            return contract.SetOCPBoolParam(accountProvider.CurrentIdentity, ocp, name, value);
        }

        public Task SetOCPIntParam(OCP ocp, string name, int value)
        {
            EnsureLoaded();
            return contract.SetOCPIntParam(accountProvider.CurrentIdentity, ocp, name, value);
        }

        public Task SetOCPStringParam(OCP ocp, string name, string value)
        {
            EnsureLoaded();
            return contract.SetOCPStringParam(accountProvider.CurrentIdentity, ocp, name, value);
        }
        #endregion

        #region Policies
        public async Task CreatePolicy(PolicyModel model)
        {
            EnsureLoaded();
            var hashCode = await hashCodeService.ComputeHashCode(model.ExternalResource);
            await contract.CreatePolicy(accountProvider.CurrentIdentity, hashCode, model.ExternalResource);
            await RefreshPolicies();
        }
        #endregion

        #region Proposals
        public async Task CreateProposal(ProposalModel model)
        {
            EnsureLoaded();
            var hashCode = await hashCodeService.ComputeHashCode(model.ExternalResource);
            await contract.CreateProposal(accountProvider.CurrentIdentity, hashCode, model.ExternalResource);
            await RefreshProposals();
        }

        public async Task ApproveProposal(Proposal proposal)
        {
            EnsureLoaded();
            await contract.ApproveProposal(accountProvider.CurrentIdentity, proposal);
            await RefreshProposals();
            await RefreshPolicies();
        }
        #endregion

        #region Refresh
        private async Task RefreshAdmins()
            => admins = (await contract.GetAdmins()).Where(a => a != emptyAddress).ToList();

        private async Task RefreshOCPs()
            => ocps = (await contract.GetOCPs()).ToList();

        private async Task RefreshPolicies()
            => policies = (await contract.GetPolicies()).ToList();

        private async Task RefreshProposals()
            => proposals = (await contract.GetProposals()).ToList();
        #endregion

        private void EnsureLoaded()
        {
            if (contract == null)
                throw new Exception("Load a contract first!");
        }
    }
}
