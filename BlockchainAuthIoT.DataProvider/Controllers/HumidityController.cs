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
    public class HumidityController : ControllerBase
    {
        private readonly ILogger<HumidityController> _logger;
        private readonly IHumidityRepository _repo;
        private readonly ITokenVerificationService _userVerification;
        private readonly IMapper _mapper;

        public HumidityController(ILogger<HumidityController> logger, IHumidityRepository repo, ITokenVerificationService userVerification)
        {
            _logger = logger;
            _repo = repo;
            _userVerification = userVerification;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<HumidityEntity, HumidityReading>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        public async Task<IEnumerable<HumidityReading>> Latest(int count = 1, string deviceNames = "")
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
            return entities.Select(e => _mapper.Map<HumidityReading>(e));
        }
    }
}
