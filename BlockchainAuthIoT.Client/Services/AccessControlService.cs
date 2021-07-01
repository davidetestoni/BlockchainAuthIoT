using BlockchainAuthIoT.Client.Models;
using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Shared;
using BlockchainAuthIoT.Shared.Repositories;
using BlockchainAuthIoT.Shared.Services;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static Nethereum.Util.UnitConversion;

namespace BlockchainAuthIoT.Client.Services
{
    public class AccessControlService
    {
        private const string emptyAddress = "0x0000000000000000000000000000000000000000";
        private readonly IWeb3Provider _web3Provider;
        private readonly IAccountProvider _accountProvider;
        private readonly IPolicyDatabase _policyDatabase;
        private AccessControl contract;
        
        private List<string> admins = new();
        private List<OCP> ocps = new();
        private List<Policy> policies = new();
        private List<Proposal> proposals = new();

        public bool ContractLoaded { get; private set; } = false;
        public string ContractAddress => contract?.Address;
        public IEnumerable<string> Admins => admins;
        public IEnumerable<OCP> OCPs => ocps;
        public IEnumerable<Policy> Policies => policies;
        public IEnumerable<Proposal> Proposals => proposals;
        public bool Initialized { get; private set; } = false;
        public bool Signed { get; private set; } = false;
        public string Signer { get; private set; } = emptyAddress;
        public string Owner { get; private set; } = emptyAddress;
        public string UserPubKey { get; private set; } = string.Empty;
        public BigInteger Price { get; private set; } = 0;
        public BigInteger AmountPaid { get; private set; } = 0;

        public AccessControlService(IWeb3Provider web3Provider, IAccountProvider accountProvider,
            IPolicyDatabase policyDatabase)
        {
            _web3Provider = web3Provider;
            _accountProvider = accountProvider;
            _policyDatabase = policyDatabase;
        }

        public async Task DeployNewContract(string signer)
        {
            var contract = await AccessControl.Deploy(_web3Provider.Web3, _accountProvider.Address, signer);
            await LoadContract(contract.Address);
        }

        public async Task LoadContract(string address)
        {
            contract = await AccessControl.FromChain(_web3Provider.Web3, address);
            ContractLoaded = true;

            Initialized = await contract.IsInitialized();
            Signer = await contract.GetSigner();
            Owner = await contract.GetOwner();
            UserPubKey = await contract.GetUserPubKey();
            Signed = await contract.IsSigned();
            Price = await contract.GetPrice();
            AmountPaid = await contract.GetAmountPaid();
            await RefreshAdmins();
            await RefreshOCPs();
            await RefreshPolicies();
            await RefreshProposals();
        }

        public void UnloadContract()
        {
            contract = null;
            ContractLoaded = false;
        }

        #region Admins and Contract Management
        public async Task AddAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.AddAdmin(_accountProvider.Address, adminAddress);
            await RefreshAdmins();
        }

        public async Task RemoveAdmin(string adminAddress)
        {
            EnsureLoaded();
            await contract.RemoveAdmin(_accountProvider.Address, adminAddress);
            await RefreshAdmins();
        }

        public async Task InitializeContract(decimal priceInEth)
        {
            EnsureLoaded();
            var wei = UnitConversion.Convert.ToWei(priceInEth, EthUnit.Ether);
            await contract.InitializeContract(_accountProvider.Address, wei);
            Initialized = await contract.IsInitialized();
            Price = await contract.GetPrice();
        }

        public async Task SignContract(string userPubKey)
        {
            EnsureLoaded();
            await contract.SignContract(_accountProvider.Address, Price, userPubKey);
            Signer = await contract.GetSigner();
            Signed = await contract.IsSigned();
            UserPubKey = await contract.GetUserPubKey();
            AmountPaid = await contract.GetAmountPaid();
        }
        #endregion

        #region OCPs (On-Chain Policies) Management
        public async Task CreateOCP(OCPModel model)
        {
            EnsureLoaded();
            await contract.CreateOCP(_accountProvider.Address, model.Resource, model.StartTime, model.Expiration);
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
            return contract.SetOCPBoolParam(_accountProvider.Address, ocp, name, value);
        }

        public Task SetOCPIntParam(OCP ocp, string name, int value)
        {
            EnsureLoaded();
            return contract.SetOCPIntParam(_accountProvider.Address, ocp, name, value);
        }

        public Task SetOCPStringParam(OCP ocp, string name, string value)
        {
            EnsureLoaded();
            return contract.SetOCPStringParam(_accountProvider.Address, ocp, name, value);
        }
        #endregion

        #region Policies
        public async Task CreatePolicy(PolicyModel model)
        {
            EnsureLoaded();
            var body = await _policyDatabase.GetPolicy(model.Location);
            var hashCode = Utils.ComputeHashCode(body);
            await contract.CreatePolicy(_accountProvider.Address, hashCode, model.Resource, model.Location);
            await RefreshPolicies();
        }
        #endregion

        #region Proposals
        public async Task CreateProposal(ProposalModel model)
        {
            EnsureLoaded();
            var body = await _policyDatabase.GetPolicy(model.Location);
            var hashCode = Utils.ComputeHashCode(body);
            var wei = UnitConversion.Convert.ToWei(model.Price, EthUnit.Ether);
            await contract.CreateProposal(_accountProvider.Address, wei, hashCode, model.Resource, model.Location);
            await RefreshProposals();
        }

        public async Task AcceptProposal(Proposal proposal)
        {
            EnsureLoaded();
            await contract.AcceptProposal(_accountProvider.Address, proposal, proposal.Price);
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
