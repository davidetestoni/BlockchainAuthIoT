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

        /// <summary>
        /// Deploys a new <see cref="AccessControl"/> contract on the blockchain.
        /// </summary>
        /// <param name="owner">The address of the account that created the contract</param>
        /// <param name="signer">The address of the user who signed the contract</param>
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

        /// <summary>
        /// Checks if the contract has been initialized.
        /// </summary>
        public Task<bool> IsInitialized()
            => contract.GetFunction("initialized").CallAsync<bool>();

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
        public async Task InitializeContract(string from)
        {
            var transactionReceipt = await contract.GetFunction("initializeContract")
                .SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Creates a policy to access a specific <paramref name="resource"/> with a given <paramref name="expiration"/>.
        /// </summary>
        public async Task<Policy> CreatePolicy(string from, string resource, DateTime startTime, DateTime expiration)
        {
            var createFunction = contract.GetFunction("createPolicy");
            var transactionReceipt = await createFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                resource, startTime.ToUnixTime(), expiration.ToUnixTime());

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

        /// <summary>
        /// Sets a boolean parameter for the given policy.
        /// </summary>
        public async Task SetPolicyBoolParam(string from, Policy policy, string name, bool value)
        {
            var setFunction = contract.GetFunction("setPolicyBoolParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                policy.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Sets an integer parameter for the given policy.
        /// </summary>
        public async Task SetPolicyIntParam(string from, Policy policy, string name, int value)
        {
            var setFunction = contract.GetFunction("setPolicyIntParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                policy.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        /// <summary>
        /// Sets a string parameter for the given policy.
        /// </summary>
        public async Task SetPolicyStringParam(string from, Policy policy, string name, string value)
        {
            var setFunction = contract.GetFunction("setPolicyStringParam");
            var transactionReceipt = await setFunction.SendTransactionAndWaitForReceiptAsync(from, gas, new HexBigInteger(0), null,
                policy.Id, name, value);

            if (!transactionReceipt.Succeeded())
            {
                throw new ContractException(transactionReceipt.Logs);
            }
        }

        public Task<bool> GetPolicyBoolParam(Policy policy, string name)
            => contract.GetFunction("getPolicyBoolParam").CallAsync<bool>(policy.Id, name);

        public Task<int> GetPolicyIntParam(Policy policy, string name)
            => contract.GetFunction("getPolicyIntParam").CallAsync<int>(policy.Id, name);

        public Task<string> GetPolicyStringParam(Policy policy, string name)
            => contract.GetFunction("getPolicyStringParam").CallAsync<string>(policy.Id, name);
    }
}
