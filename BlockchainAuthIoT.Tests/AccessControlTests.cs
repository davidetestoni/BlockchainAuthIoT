using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Exceptions;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Core.Utils;
using Nethereum.Web3;
using System;
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

        #region Policy Management
        [Fact]
        public async Task CreatePolicy_FromAdmin_Ok()
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            var ac = await DeployContract();
            var policy = await ac.CreatePolicy(owner, resource, startTime, expiration);

            Assert.Equal(resource, policy.Resource);
            Assert.Equal(expiration, policy.Expiration);
            Assert.Equal(startTime, policy.StartTime);
        }

        [Fact]
        public async Task CreatePolicy_FromSigner_Throws()
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            var ac = await DeployContract();
            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.CreatePolicy(signer, resource, startTime, expiration));
        }

        [Fact]
        public async Task SetPolicyBoolParam_SetAndGet_SameValue()
        {
            var value = true;
            var name = "myBool";
            var ac = await DeployContract();
            var policy = await CreateDummyPolicy(ac, owner);

            await ac.SetPolicyBoolParam(owner, policy, name, value);
            Assert.Equal(value, await ac.GetPolicyBoolParam(policy, name));
        }

        [Fact]
        public async Task SetPolicyIntParam_SetAndGet_SameValue()
        {
            int value = 42;
            var name = "myInt";
            var ac = await DeployContract();
            var policy = await CreateDummyPolicy(ac, owner);

            await ac.SetPolicyIntParam(owner, policy, name, value);
            Assert.Equal(value, await ac.GetPolicyIntParam(policy, name));
        }

        [Fact]
        public async Task SetPolicyStringParam_SetAndGet_SameValue()
        {
            string value = "dev";
            var name = "myString";
            var ac = await DeployContract();
            var policy = await CreateDummyPolicy(ac, owner);

            await ac.SetPolicyStringParam(owner, policy, name, value);
            Assert.Equal(value, await ac.GetPolicyStringParam(policy, name));
        }

        [Fact]
        public async Task SetPolicyBoolParam_FromSigner_Throws()
        {
            bool value = true;
            var name = "myBool";
            var ac = await DeployContract();
            var policy = await CreateDummyPolicy(ac, owner);

            await Assert.ThrowsAsync<ContractException>(
                async () => await ac.SetPolicyBoolParam(signer, policy, name, value));
        }
        #endregion

        private Task<AccessControl> DeployContract()
            => AccessControl.Deploy(web3Fixture.Web3, owner, signer);

        private static async Task<Policy> CreateDummyPolicy(AccessControl ac, string from)
        {
            var resource = "test";
            var startTime = DateTime.UtcNow.TrimMilliseconds();
            var expiration = startTime.AddDays(7);

            return await ac.CreatePolicy(from, resource, startTime, expiration);
        }
    }
}
