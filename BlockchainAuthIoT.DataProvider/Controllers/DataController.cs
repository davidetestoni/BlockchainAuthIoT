using AutoMapper;
using BlockchainAuthIoT.DataProvider.Entities;
using BlockchainAuthIoT.DataProvider.Repositories;
using BlockchainAuthIoT.DataProvider.Services;
using BlockchainAuthIoT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockchainAuthIoT.DataProvider.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IDataRepository _repo;
        private readonly IUserVerificationService _userVerification;
        private readonly IMapper _mapper;

        public DataController(ILogger<DataController> logger, IDataRepository repo, IUserVerificationService userVerification)
        {
            _logger = logger;
            _repo = repo;
            _userVerification = userVerification;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DataEntity, SampleData>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        public async Task<IEnumerable<SampleData>> GetAll()
        {
            // TODO: Move all this to a middleware
            // Check if the request has the required header
            if (!Request.Headers.ContainsKey("Token"))
                return Array.Empty<SampleData>();

            // Verify the token
            await _userVerification.VerifyToken(Request.Headers["Token"]);

            // TODO: Verify the policies

            // Send the data
            var entities = await _repo.GetAll().ToListAsync();
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return entities.Select(e => _mapper.Map<SampleData>(e));
        }
    }
}
