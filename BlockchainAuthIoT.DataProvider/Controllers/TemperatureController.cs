using AutoMapper;
using BlockchainAuthIoT.DataProvider.Entities;
using BlockchainAuthIoT.DataProvider.Repositories;
using BlockchainAuthIoT.DataProvider.Services;
using BlockchainAuthIoT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TemperatureController : ControllerBase
    {
        private readonly ILogger<TemperatureController> _logger;
        private readonly ITemperatureRepository _repo;
        private readonly ITokenVerificationService _userVerification;
        private readonly IMapper _mapper;

        public TemperatureController(ILogger<TemperatureController> logger, ITemperatureRepository repo, ITokenVerificationService userVerification)
        {
            _logger = logger;
            _repo = repo;
            _userVerification = userVerification;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TemperatureEntity, TemperatureReading>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        public async Task<IEnumerable<TemperatureReading>> Latest(int count = 1, string deviceNames = "")
        {
            // TODO: Move all this to a middleware
            // Check if the request has the required header
            if (!Request.Headers.ContainsKey("Token"))
            {
                Unauthorized("Missing Token header");
            }

            // Verify the token
            try
            {
                await _userVerification.VerifyToken(Request.Headers["Token"]);
            }
            catch
            {
                Unauthorized("Token verification failed");
            }

            // TODO: Verify the policies

            // Send the data
            var entities = await _repo.GetLatest(count, deviceNames.Split(','));
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return entities.Select(e => _mapper.Map<TemperatureReading>(e));
        }
    }
}
