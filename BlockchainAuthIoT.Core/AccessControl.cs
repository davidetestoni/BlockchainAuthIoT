using BlockchainAuthIoT.Core.Exceptions;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.Core.Utils;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.Core
{
    public class AccessControl
    {
        private readonly Contract contract;
        private static readonly HexBigInteger gas = new(10 * 1000 * 1000);

        /// <summary>
        /// The public address of the contract on the blockchain.
        /// </summary>
        public string Address => contract.Address;

        /// <summary>
        /// The owner of the contract.
        /// </summary>
        public string Owner { get;  init; }

        public AccessControl(Contract contract)
        {
            this.contract = contract;
        }

        #region Deployment and Loading
        /// <summary>
        /// Deploys a new <see cref="AccessControl"/> contract on the blockchain.
        /// </summary>
        /// <param name="owner">The address of the account that created the contract</param>
        /// <param name="signer">The address of the signer of the contract</param>
        public static async Task<AccessControl> Deploy(Web3 web3, string owner, string signer)
        {
            // Read the ABI and the bytecode from the solidity compiler output files
            var abi = File.ReadAllText(Path.Combine("Contracts", "bin", "AccessControl.abi"));
            var bytecode = File.ReadAllText(Path.Combine("Contracts", "bin", "AccessControl.bin"));

            // Deploy the contract and get the address
            var transactionReceipt = await web3.Eth.DeployContract
                .SendRequestAndWaitForReceiptAsync(abi, bytecode, owner, gas, new HexBigInteger(0), null, signer);
            var contractAddress = transactionReceipt.ContractAddress;

            // Get the contract
            var contract = web3.Eth.GetContract(abi, contractAddress);
            return new AccessControl(contract);
        }

        /// <summary>
        /// Retrieves an <see cref="AccessControl"/> contract from the blockchain at a given
        /// <paramref name="address"/>.
        /// </summary>
        public static async Task<AccessControl> FromChain(Web3 web3, string address)
        {
            // Read the ABI and the bytecode from the solidity compiler output files
            var abi = File.ReadAllText(Path.Combine("Contracts", "bin", "AccessControl.abi"));

            // Get the contract
            var contract = web3.Eth.GetContract(abi, address);

            return new AccessControl(contract)
            {
                Owner = await contract.GetFunction("owner").CallAsync<string>()
            };
        }
        #endregion

        #region Admins and Contract Management
        /// <summary>
        /// Checks if the contract has been initialized.
        /// </summary>
        public Task<bool> IsInitialized()
            => contract.GetFunction("initialized").CallAsync<bool>();

        /// <summary>
        /// Checks if the contract has been signed.
        /// </summary>
        public Task<bool> IsSigned()
            => contract.GetFunction("signed").CallAsync<bool>();

        /// <summary>
        /// Gets the price of the contract.
        /// </summary>
        public Task<BigInteger> GetPrice()
            => contract.GetFunction("price").CallAsync<BigInteger>();

        /// <summary>
        /// Gets the amount that the signer paid so far.
        /// </summary>
        public Task<BigInteger> GetAmountPaid()
            => contract.GetFunction("amountPaid").CallAsync<BigInteger>();

        /// <summary>
        /// Gets the address of the signer of the contract.
        /// </summary>
        public Task<string> GetSigner()
            => contract.GetFunction("signer").CallAsync<string>();

        /// <summary>
        /// Gets the list of all appointed admins.
        /// </summary>
        public async Task<string[]> GetAdmins()
        {
            var countFunction = contract.GetFunction("adminsCount");
            var adminsCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("admins");

            var admins = new List<string>();
            for (int i = 0; i < adminsCount; i++)
            {
                var address = await function.CallAsync<string>(i);
                admins.Add(address);
            }

            return admins.ToArray();
        }

        /// <summary>
        /// Adds the given <paramref name="adminAddress"/> to the admins (only an admin can perform this).
        /// </summary>
        public async Task AddAdmin(string from, string adminAddress)
        {
            var transactionReceipt = await contract.GetFunction("addAdmin")
                .SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                adminAddress);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Removes an <paramref name="adminAddress"/> from the admins (only the owner can perform this).
        /// </summary>
        public async Task RemoveAdmin(string from, string adminAddress)
        {
            var transactionReceipt = await contract.GetFunction("removeAdmin")
                .SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                adminAddress);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Initializes the contract and locks all the policies (only an admin can perform this)
        /// (the contract must not be initialized yet).
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task InitializeContract(string from, BigInteger price)
        {
            var transactionReceipt = await contract.GetFunction("initializeContract")
                .SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null, price);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Signs the contract by sending the <paramref name="amount"/> required. After being signed, the policies defined
        /// in the contract are in effect and can be used to provide access control capabilities.
        /// </summary>
        /// <remarks>Only the signer can perform this</remarks>
        public async Task SignContract(string from, BigInteger amount)
        {
            var transactionReceipt = await contract.GetFunction("signContract")
                .SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(amount), null);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }
        #endregion

        #region OCPs (On-Chain Policies) Management
        /// <summary>
        /// Gets the list of all on-chain policies.
        /// </summary>
        public async Task<OCP[]> GetOCPs()
        {
            var countFunction = contract.GetFunction("ocpsCount");
            var ocpsCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("ocps");

            var ocps = new List<OCP>();
            for (int i = 0; i < ocpsCount; i++)
            {
                var ocp = await function.CallDeserializingToObjectAsync<OCP>(i);
                ocps.Add(ocp);
            }

            return ocps.ToArray();
        }

        /// <summary>
        /// Creates an on-chain policy to access a specific <paramref name="resource"/> with a given <paramref name="expiration"/>.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task<OCP> CreateOCP(string from, string resource, DateTime startTime, DateTime expiration)
        {
            var createFunction = contract.GetFunction("createOCP");
            var transactionReceipt = await createFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                resource, startTime.ToUnixTime(), expiration.ToUnixTime());

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }

            // This is the ID of the next ocp so we need to subtract 1
            var ocpsCount = await contract.GetFunction("ocpsCount").CallAsync<uint>();
            var ocpId = ocpsCount - 1;

            var getFunction = contract.GetFunction("ocps");
            var ocp = await getFunction.CallDeserializingToObjectAsync<OCP>(ocpId);
            ocp.Id = ocpId;

            return ocp;
        }

        /// <summary>
        /// Sets a boolean parameter for the given on-chain policy.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task SetOCPBoolParam(string from, OCP ocp, string name, bool value)
        {
            var setFunction = contract.GetFunction("setOCPBoolParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                ocp.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Sets an integer parameter for the given on-chain policy.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task SetOCPIntParam(string from, OCP ocp, string name, int value)
        {
            var setFunction = contract.GetFunction("setOCPIntParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                ocp.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Sets a string parameter for the given on-chain policy.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task SetOCPStringParam(string from, OCP ocp, string name, string value)
        {
            var setFunction = contract.GetFunction("setOCPStringParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                ocp.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Gets the value of a boolean parameter for the given <paramref name="ocp"/>.
        /// </summary>
        public Task<bool> GetOCPBoolParam(OCP ocp, string name)
            => contract.GetFunction("getOCPBoolParam").CallAsync<bool>(ocp.Id, name);

        /// <summary>
        /// Gets the value of an integer parameter for the given <paramref name="ocp"/>.
        /// </summary>
        public Task<int> GetOCPIntParam(OCP ocp, string name)
            => contract.GetFunction("getOCPIntParam").CallAsync<int>(ocp.Id, name);

        /// <summary>
        /// Gets the value of a string parameter for the given <paramref name="ocp"/>.
        /// </summary>
        public Task<string> GetOCPStringParam(OCP ocp, string name)
            => contract.GetFunction("getOCPStringParam").CallAsync<string>(ocp.Id, name);
        #endregion

        #region Policies Management
        /// <summary>
        /// Gets the list of all policies.
        /// </summary>
        public async Task<Policy[]> GetPolicies()
        {
            var countFunction = contract.GetFunction("policiesCount");
            var policiesCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("policies");

            var policies = new List<Policy>();
            for (int i = 0; i < policiesCount; i++)
            {
                var policy = await function.CallDeserializingToObjectAsync<Policy>(i);
                policies.Add(policy);
            }

            return policies.ToArray();
        }

        /// <summary>
        /// Creates a new policy given a <paramref name="hashCode"/> for validation, the <paramref name="resource"/>
        /// that this proposal refers to, and and the <paramref name="location"/> where the body of the policy can be found.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task<Policy> CreatePolicy(string from, byte[] hashCode, string resource, string location)
        {
            if (hashCode.Length != 32)
                throw new ArgumentException("Must be 32 bytes long", nameof(hashCode));

            var createFunction = contract.GetFunction("createPolicy");
            var transactionReceipt = await createFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                hashCode, resource, location);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }

            // This is the ID of the next policy so we need to subtract 1
            var policiesCount = await contract.GetFunction("policiesCount").CallAsync<uint>();
            var policyId = policiesCount - 1;

            var getFunction = contract.GetFunction("policies");
            var policy = await getFunction.CallDeserializingToObjectAsync<Policy>(policyId);
            policy.Id = policyId;

            return policy;
        }
        #endregion

        #region Proposal Management
        /// <summary>
        /// Gets the list of all proposals.
        /// </summary>
        public async Task<Proposal[]> GetProposals()
        {
            var countFunction = contract.GetFunction("proposalsCount");
            var proposalsCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("proposals");

            var proposals = new List<Proposal>();
            for (int i = 0; i < proposalsCount; i++)
            {
                var proposal = await function.CallDeserializingToObjectAsync<Proposal>(i);
                proposals.Add(proposal);
            }

            return proposals.ToArray();
        }

        /// <summary>
        /// Creates a new proposal given a <paramref name="hashCode"/> for validation, the <paramref name="resource"/>
        /// that this proposal refers to, and and the <paramref name="location"/> where the body of the proposal can be found.
        /// </summary>
        /// <remarks>Only an admin can perform this</remarks>
        public async Task<Proposal> CreateProposal(string from, BigInteger price, byte[] hashCode, string resource, string location)
        {
            var createFunction = contract.GetFunction("createProposal");
            var transactionReceipt = await createFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                price, hashCode, resource, location);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }

            // This is the ID of the next proposal so we need to subtract 1
            var proposalsCount = await contract.GetFunction("proposalsCount").CallAsync<uint>();
            var proposalId = proposalsCount - 1;

            var getFunction = contract.GetFunction("proposals");
            var proposal = await getFunction.CallDeserializingToObjectAsync<Proposal>(proposalId);
            proposal.Id = proposalId;

            return proposal;
        }

        /// <summary>
        /// Accepts a given proposal by paying the required amount and turns it into a policy.
        /// </summary>
        /// <remarks>Only the signer can perform this</remarks>
        public async Task AcceptProposal(string from, Proposal proposal, BigInteger amount)
        {
            var acceptFunction = contract.GetFunction("acceptProposal");
            var transactionReceipt = await acceptFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(amount), null,
                proposal.Id);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }
        #endregion
    }
}
