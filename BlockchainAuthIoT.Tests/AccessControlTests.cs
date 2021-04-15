using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Exceptions;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Core.Utils;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static Nethereum.Util.UnitConversion;

namespace BlockchainAuthIoT.Tests
{
    // Execute this once before all tests
    public class Web3Fixture
    {
        public string[] Accounts { get; private set; }
        public Web3 Web3 { get; private set; }

        public Web3Fixture()
        {
            // Initialize web3 on the default port (expecting a testchain to be there)
            Web3 = new Web3();

            // Get the premade accounts and unlock them for 10 minutes
            Accounts = Web3.Eth.Accounts.SendRequestAsync().Result;
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[0], "password", 10 * 60).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[1], "password", 10 * 60).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[2], "password", 10 * 60).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[3], "password", 10 * 60).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[4], "password", 10 * 60).Wait();
        }
    }

    public class AccessControlTests : IClassFixture<Web3Fixture>
    {
        private readonly Web3Fixture web3Fixture;
        private readonly string owner;
        private readonly string signer;

        public AccessControlTests(Web3Fixture fixture)
        {
            web3Fixture = fixture;
            owner = fixture.Accounts[0];
            signer = fixture.Accounts[1];
        }

        #region Deployment, Initialization and Signature
        [Fact]
        public async Task Deploy_Default_NotInitialized()
        {
            var ac = await DeployContract();
            
            var initialized = await ac.IsInitialized();
            Assert.False(initialized);
        }

        [Fact]
        public async Task Deploy_Default_NotSigned()
        {
            var ac = await DeployContract();

            var signed = await ac.IsSigned();
            Assert.False(signed);
        }

        [Fact]
        public async Task Deploy_Default_SignerIsCorrect()
        {
            var ac = await DeployContract();

            var signerInContract = await ac.GetSigner();
            Assert.Equal(signer, signerInContract);
        }

        [Fact]
        public async Task InitializeContract_FromOwner_Initialized()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);

            var initialized = await ac.IsInitialized();
            Assert.True(initialized);
        }

        [Fact]
        public async Task InitializeContract_FromSigner_Throws()
        {
            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.InitializeContract(signer, 0));
        }

        [Fact]
        public async Task SignContract_FromSigner_EnoughFunds_Signed()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner, 20);
            await ac.SignContract(signer, 20);

            var signed = await ac.IsSigned();
            Assert.True(signed);
        }

        [Fact]
        public async Task SignContract_FromSigner_NotEnoughFunds_NotSigned()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner, 20);
            await ac.SignContract(signer, 10);

            var signed = await ac.IsSigned();
            Assert.False(signed);

            var amountPaid = await ac.GetAmountPaid();
            Assert.Equal(10, amountPaid);
        }

        [Fact]
        public async Task SignContract_FromSigner_TwoPayments_Signed()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner, 20);
            await ac.SignContract(signer, 10);
            await ac.SignContract(signer, 10);

            var signed = await ac.IsSigned();
            Assert.True(signed);
        }

        [Fact]
        public async Task SignContract_FromOwner_Throws()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner, 20);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.SignContract(owner, 20));
        }

        [Fact]
        public async Task SignContract_Signed_OwnerReceivesFunds()
        {
            var contractPrice = Web3.Convert.ToWei(0.001, EthUnit.Ether);

            var ac = await DeployContract();
            await ac.InitializeContract(owner, contractPrice);

            var initialBalance = await GetBalance(owner);

            await ac.SignContract(signer, contractPrice);
            var finalBalance = await GetBalance(owner);

            Assert.True(finalBalance.Value - initialBalance.Value > contractPrice);
        }
        #endregion

        #region Admins Management
        [Fact]
        public async Task AddAdmin_FromAdmin_Ok()
        {
            var newAdmin = web3Fixture.Accounts[2];
            var ac = await DeployContract();
            await ac.AddAdmin(owner, newAdmin);

            var admins = await ac.GetAdmins();
            Assert.Contains(newAdmin, admins);
        }

        [Fact]
        public async Task AddAdmin_FromSigner_Throws()
        {
            var newAdmin = web3Fixture.Accounts[2];
            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.AddAdmin(signer, newAdmin));
        }

        [Fact]
        public async Task RemoveAdmin_FromOwner_Ok()
        {
            var newAdmin = web3Fixture.Accounts[2];
            var ac = await DeployContract();
            await ac.AddAdmin(owner, newAdmin);
            
            var admins = await ac.GetAdmins();
            Assert.Contains(newAdmin, admins);

            await ac.RemoveAdmin(owner, newAdmin);
            admins = await ac.GetAdmins();
            Assert.DoesNotContain(newAdmin, admins);
        }

        [Fact]
        public async Task RemoveAdmin_FromSigner_Throws()
        {
            var newAdmin = web3Fixture.Accounts[2];
            var ac = await DeployContract();
            await ac.AddAdmin(owner, newAdmin);

            var admins = await ac.GetAdmins();
            Assert.Contains(newAdmin, admins);

            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.RemoveAdmin(signer, newAdmin));
        }
        #endregion

        #region OCP Management
        [Fact]
        public async Task CreateOCP_FromAdmin_Ok()
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            var ac = await DeployContract();
            var ocp = await ac.CreateOCP(owner, resource, startTime, expiration);

            Assert.Equal(resource, ocp.Resource);
            Assert.Equal(expiration, ocp.Expiration);
            Assert.Equal(startTime, ocp.StartTime);
        }

        [Fact]
        public async Task CreateOCP_FromAdminAfterInitialization_Throws()
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateOCP(owner, resource, startTime, expiration));
        }

        [Fact]
        public async Task CreateOCP_FromSigner_Throws()
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateOCP(signer, resource, startTime, expiration));
        }

        [Fact]
        public async Task SetOCPBoolParam_SetAndGet_SameValue()
        {
            var value = true;
            var name = "myBool";
            var ac = await DeployContract();
            var ocp = await CreateDummyOCP(ac, owner);

            await ac.SetOCPBoolParam(owner, ocp, name, value);
            Assert.Equal(value, await ac.GetOCPBoolParam(ocp, name));
        }

        [Fact]
        public async Task SetOCPIntParam_SetAndGet_SameValue()
        {
            int value = 42;
            var name = "myInt";
            var ac = await DeployContract();
            var ocp = await CreateDummyOCP(ac, owner);

            await ac.SetOCPIntParam(owner, ocp, name, value);
            Assert.Equal(value, await ac.GetOCPIntParam(ocp, name));
        }

        [Fact]
        public async Task SetOCPStringParam_SetAndGet_SameValue()
        {
            string value = "dev";
            var name = "myString";
            var ac = await DeployContract();
            var ocp = await CreateDummyOCP(ac, owner);

            await ac.SetOCPStringParam(owner, ocp, name, value);
            Assert.Equal(value, await ac.GetOCPStringParam(ocp, name));
        }

        [Fact]
        public async Task SetOCPBoolParam_FromSigner_Throws()
        {
            bool value = true;
            var name = "myBool";
            var ac = await DeployContract();
            var ocp = await CreateDummyOCP(ac, owner);

            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.SetOCPBoolParam(signer, ocp, name, value));
        }
        #endregion

        #region Policy Management
        [Fact]
        public async Task CreatePolicy_FromAdmin_Ok()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            var policy = await ac.CreatePolicy(owner, hashCode, externalResource);

            Assert.True(hashCode.SequenceEqual(policy.HashCode));
            Assert.Equal(externalResource, policy.ExternalResource);
        }

        [Fact]
        public async Task CreatePolicy_FromAdminAfterInitialization_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreatePolicy(owner, hashCode, externalResource));
        }

        [Fact]
        public async Task CreatePolicy_FromSigner_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreatePolicy(signer, hashCode, externalResource));
        }
        #endregion

        #region Proposal Management
        [Fact]
        public async Task CreateProposal_FromAdmin_Ok()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, 0, hashCode, externalResource);

            Assert.False(proposal.Accepted);
            Assert.True(hashCode.SequenceEqual(proposal.HashCode));
            Assert.Equal(externalResource, proposal.ExternalResource);
        }

        [Fact]
        public async Task CreateProposal_FromAdminBeforeSignature_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateProposal(owner, 0, hashCode, externalResource));
        }

        [Fact]
        public async Task CreateProposal_FromSigner_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateProposal(signer, 0, hashCode, externalResource));
        }

        [Fact]
        public async Task AcceptProposal_FromSigner_EnoughFunds_Accepted()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, 20, hashCode, externalResource);

            await ac.AcceptProposal(signer, proposal, 20);
            var proposals = await ac.GetProposals();
            var acceptedProposal = proposals.Last();

            Assert.True(acceptedProposal.Accepted);

            var policies = await ac.GetPolicies();
            var newPolicy = policies.Last();

            Assert.True(hashCode.SequenceEqual(newPolicy.HashCode));
            Assert.Equal(externalResource, newPolicy.ExternalResource);
        }

        [Fact]
        public async Task AcceptProposal_FromSigner_NotEnoughFunds_NotAccepted()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, 20, hashCode, externalResource);

            await ac.AcceptProposal(signer, proposal, 10);
            var proposals = await ac.GetProposals();
            var acceptedProposal = proposals.Last();

            Assert.False(acceptedProposal.Accepted);
        }

        [Fact]
        public async Task AcceptProposal_FromSigner_TwoPayments_Accepted()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, 20, hashCode, externalResource);

            await ac.AcceptProposal(signer, proposal, 10);
            await ac.AcceptProposal(signer, proposal, 10);
            var proposals = await ac.GetProposals();
            var acceptedProposal = proposals.Last();

            Assert.True(acceptedProposal.Accepted);
        }

        [Fact]
        public async Task AcceptProposal_Accepted_OwnerReceivesFunds()
        {
            var proposalPrice = Web3.Convert.ToWei(0.0003, EthUnit.Ether);

            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, proposalPrice, hashCode, externalResource);

            var initialBalance = await GetBalance(owner);

            await ac.AcceptProposal(signer, proposal, proposalPrice);
            var finalBalance = await GetBalance(owner);

            Assert.True(finalBalance.Value - initialBalance.Value > proposalPrice);
        }

        [Fact]
        public async Task AcceptProposal_FromAdmin_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner, 0);
            await ac.SignContract(signer, 0);
            var proposal = await ac.CreateProposal(owner, 0, hashCode, externalResource);

            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.AcceptProposal(owner, proposal, 0));
        }
        #endregion

        private Task<AccessControl> DeployContract()
            => AccessControl.Deploy(web3Fixture.Web3, owner, signer);

        private Task<HexBigInteger> GetBalance(string address)
            => web3Fixture.Web3.Eth.GetBalance.SendRequestAsync(address);

        private static Task<OCP> CreateDummyOCP(AccessControl ac, string from)
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            return ac.CreateOCP(from, resource, startTime, expiration);
        }
    }
}
