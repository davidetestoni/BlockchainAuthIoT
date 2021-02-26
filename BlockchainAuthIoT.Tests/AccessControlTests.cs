using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Exceptions;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Core.Utils;
using Nethereum.Web3;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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

            // Get the premade accounts and unlock them
            Accounts = Web3.Eth.Accounts.SendRequestAsync().Result;
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[0], "password", 120).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[1], "password", 120).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[2], "password", 120).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[3], "password", 120).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[4], "password", 120).Wait();
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

        #region Deployment and Initialization
        [Fact]
        public async Task Deploy_Default_NotInitialized()
        {
            var ac = await DeployContract();
            
            var initialized = await ac.IsInitialized();
            Assert.False(initialized);
        }

        [Fact]
        public async Task InitializeContract_FromOwner_Ok()
        {
            var ac = await DeployContract();
            await ac.InitializeContract(owner);

            var initialized = await ac.IsInitialized();
            Assert.True(initialized);
        }

        [Fact]
        public async Task InitializeContract_FromSigner_Throws()
        {
            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.InitializeContract(signer));
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
            await ac.InitializeContract(owner);
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
            await ac.InitializeContract(owner);
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
        public async Task CreateProposal_FromSigner_Ok()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner);
            var proposal = await ac.CreateProposal(signer, hashCode, externalResource);

            Assert.False(proposal.Approved);
            Assert.True(hashCode.SequenceEqual(proposal.HashCode));
            Assert.Equal(externalResource, proposal.ExternalResource);
        }

        [Fact]
        public async Task CreateProposal_FromSignerBeforeInitialization_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateProposal(signer, hashCode, externalResource));
        }

        [Fact]
        public async Task CreateProposal_FromAdmin_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner);
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreateProposal(owner, hashCode, externalResource));
        }

        [Fact]
        public async Task ApproveProposal_FromAdmin_Ok()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner);
            var proposal = await ac.CreateProposal(signer, hashCode, externalResource);

            await ac.ApproveProposal(owner, proposal);
            var proposals = await ac.GetProposals();
            var approvedProposal = proposals.Last();

            Assert.True(approvedProposal.Approved);

            var policies = await ac.GetPolicies();
            var newPolicy = policies.Last();

            Assert.True(hashCode.SequenceEqual(newPolicy.HashCode));
            Assert.Equal(externalResource, newPolicy.ExternalResource);
        }

        [Fact]
        public async Task ApproveProposal_FromSigner_Throws()
        {
            var random = new Random();
            var hashCode = new byte[32];
            random.NextBytes(hashCode);
            var externalResource = "test";

            var ac = await DeployContract();
            await ac.InitializeContract(owner);
            var proposal = await ac.CreateProposal(signer, hashCode, externalResource);

            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.ApproveProposal(signer, proposal));
        }
        #endregion

        private Task<AccessControl> DeployContract()
            => AccessControl.Deploy(web3Fixture.Web3, owner, signer);

        private static Task<OCP> CreateDummyOCP(AccessControl ac, string from)
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            return ac.CreateOCP(from, resource, startTime, expiration);
        }
    }
}
