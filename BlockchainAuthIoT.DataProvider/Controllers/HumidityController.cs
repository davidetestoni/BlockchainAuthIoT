using AutoMapper;
using BlockchainAuthIoT.DataProvider.Entities;
using BlockchainAuthIoT.DataProvider.Exceptions;
using BlockchainAuthIoT.DataProvider.Models.Policies.Rules;
using BlockchainAuthIoT.DataProvider.Repositories;
using BlockchainAuthIoT.DataProvider.Services;
using BlockchainAuthIoT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HumidityController : ControllerBase
    {
        private readonly ILogger<HumidityController> _logger;
        private readonly IHumidityRepository _repo;
        private readonly ITokenVerificationService _tokenVerification;
        private readonly IPolicyVerificationService _policyVerification;
        private readonly IMapper _mapper;

        public HumidityController(ILogger<HumidityController> logger, IHumidityRepository repo,
            ITokenVerificationService tokenVerification, IPolicyVerificationService policyVerification)
        {
            _logger = logger;
            _repo = repo;
            _tokenVerification = tokenVerification;
            _policyVerification = policyVerification;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<HumidityEntity, HumidityReading>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        public async Task<ObjectResult> Latest(int count = 1, string deviceNames = "")
        {
            // TODO: Move the token verification to a middleware
            // Check if the request has the required header
            if (!Request.Headers.ContainsKey("Token"))
            {
                Unauthorized("Missing Token header");
            }

            // Verify the token
            string contractAddress = string.Empty;
            try
            {
                contractAddress = await _tokenVerification.VerifyToken(Request.Headers["Token"]);
            }
            catch (TokenVerificationException ex)
            {
                return new ObjectResult($"Token verification failed: {ex.Message}") { StatusCode = 401 };
            }

            // Verify the policies
            var deviceList = deviceNames.Split(',');
            try
            {
                await _policyVerification.VerifyPolicy(contractAddress, "humidity/latest", new()
                {
                    new IntPolicyRule("max_items", max => count <= max),
                    new StringPolicyRule("devices", allowed =>
                    {
                        var allowedList = allowed.Split(',');
                        return deviceList.All(d => allowedList.Contains(d));
                    })
                });
            }
            catch (PolicyVerificationException ex)
            {
                return new ObjectResult($"Policy verification failed: {ex.Message}") { StatusCode = 403 };
            }
            catch (PolicyRuleVerificationException ex)
            {
                return new ObjectResult($"Policy verification failed: {ex.Message}") { StatusCode = 403 };
            }
            catch (ContractNotFoundException)
            {
                return new ObjectResult($"Contract not found") { StatusCode = 404 };
            }
            catch (PolicyNotFoundException ex)
            {
                return new ObjectResult($"The policy for resource {ex.Resource} was not found") { StatusCode = 404 };
            }

            // Send the data
            var entities = await _repo.GetLatest(count, deviceList);
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return new ObjectResult(entities.Select(e => _mapper.Map<HumidityReading>(e)));
        }
    }
}
