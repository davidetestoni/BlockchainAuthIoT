﻿using BlockchainAuthIoT.Core;
using BlockchainAuthIoT.Core.Models;
using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Extensions;
using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using BlockchainAuthIoT.Shared;
using BlockchainAuthIoT.Shared.Repositories;
using BlockchainAuthIoT.Shared.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Services
{
    public class PolicyVerificationService : IPolicyVerificationService
    {
        private readonly IDistributedCache _cache;
        private readonly IWeb3Provider _web3Provider;
        private readonly IPolicyDatabase _policyDatabase;

        public TimeSpan PolicyValidity { get; set; } = TimeSpan.FromHours(1);

        public PolicyVerificationService(IDistributedCache cache, IWeb3Provider web3Provider, IPolicyDatabase policyDatabase)
        {
            _cache = cache;
            _web3Provider = web3Provider;
            _policyDatabase = policyDatabase;
        }

        public async Task VerifyPolicy(string contractAddress, string resource, List<PolicyRule> rules)
        {
            // Search for a cached policy
            var json = await _cache.GetRecordAsync<string>($"{contractAddress}_{resource}");

            // If there is no policy in the cache
            if (json is null)
            {
                // Get the policies from the blockchain and search for a policy for the requested resource
                AccessControl ac;

                try
                {
                    ac = await AccessControl.FromChain(_web3Provider.Web3, contractAddress);
                }
                catch
                {
                    throw new ContractNotFoundException(contractAddress);
                }

                var contractPolicies = await ac.GetPolicies();
                var contractPolicy = contractPolicies.FirstOrDefault(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));

                // If there is none, then verify the OCP
                if (contractPolicy is null)
                {
                    await VerifyOCP(ac, resource, rules);
                    return;
                }

                // Otherwise retrieve the json from the remote policy database
                var body = await _policyDatabase.GetPolicy(contractPolicy.Location);
                json = Encoding.UTF8.GetString(body);

                // Verify that the policy hasn't been modified
                if (!contractPolicy.HashCode.SequenceEqual(Utils.ComputeHashCode(body)))
                {
                    throw new PolicyVerificationException(resource, "Hashcode mismatch. The body of the policy might have been altered");
                }

                // Save it to the cache for later use
                await _cache.SetRecordAsync($"{contractAddress}_{resource}", json, PolicyValidity, PolicyValidity);
            }

            var policy = JObject.Parse(json);

            // Verify that the validity period has already started
            var startTime = policy["start_time"].ToObject<DateTime>();

            if (DateTime.UtcNow < startTime)
            {
                throw new PolicyVerificationException(resource, $"The validity period will start on {startTime}");
            }

            // Verify that the validity period hasn't ended yet
            var expiration = policy["expiration"].ToObject<DateTime>();

            if (DateTime.UtcNow > expiration)
            {
                throw new PolicyVerificationException(resource, $"The validity period ended on {expiration}");
            }

            // Verify all the rules
            foreach (var rule in rules)
            {
                var respected = false;

                try
                {
                    respected = rule switch
                    {
                        BoolPolicyRule x => x.Function.Invoke(policy.Value<bool>(rule.Parameter)),
                        IntPolicyRule x => x.Function.Invoke(policy.Value<int>(rule.Parameter)),
                        StringPolicyRule x => x.Function.Invoke(policy.Value<string>(rule.Parameter)),
                        _ => throw new NotImplementedException(),
                    };
                }
                catch
                {
                    // If something goes wrong, assume it's invalid
                }

                if (!respected)
                {
                    throw new PolicyRuleVerificationException(resource, rule.Parameter);
                }
            }
        }

        private async Task VerifyOCP(AccessControl ac, string resource, List<PolicyRule> rules)
        {
            OCP ocp;
            var json = await _cache.GetRecordAsync<string>($"{ac.Address}_ocp_{resource}");
            
            if (json is not null)
            {
                ocp = JsonConvert.DeserializeObject<OCP>(json);
            }
            else
            {
                var ocps = await ac.GetOCPs();
                ocp = ocps.FirstOrDefault(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));

                if (ocp is null)
                {
                    throw new PolicyNotFoundException(resource);
                }

                // Save it to the cache for later use
                json = JsonConvert.SerializeObject(ocp);
                await _cache.SetRecordAsync($"{ac.Address}_ocp_{resource}", json, PolicyValidity, PolicyValidity);
            }

            if (DateTime.UtcNow < ocp.StartTime)
            {
                throw new PolicyVerificationException(resource, $"The validity period will start on {ocp.StartTime}");
            }

            if (DateTime.UtcNow > ocp.Expiration)
            {
                throw new PolicyVerificationException(resource, $"The validity period ended on {ocp.Expiration}");
            }

            // Verify all the rules
            foreach (var rule in rules)
            {
                var respected = false;

                try
                {
                    // TODO: Add caching for OCP parameters as well
                    switch (rule)
                    {
                        case BoolPolicyRule x:
                            var boolValue = await ac.GetOCPBoolParam(ocp, rule.Parameter);
                            respected = x.Function.Invoke(boolValue);
                            break;

                        case IntPolicyRule x:
                            var intValue = await ac.GetOCPIntParam(ocp, rule.Parameter);
                            respected = x.Function.Invoke(intValue);
                            break;

                        case StringPolicyRule x:
                            var stringValue = await ac.GetOCPStringParam(ocp, rule.Parameter);
                            respected = x.Function.Invoke(stringValue);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                catch
                {
                    // If something goes wrong, assume it's invalid
                }

                if (!respected)
                {
                    throw new PolicyRuleVerificationException(resource, rule.Parameter);
                }
            }
        }
    }
}
