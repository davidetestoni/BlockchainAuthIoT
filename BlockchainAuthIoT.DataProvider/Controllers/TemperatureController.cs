using AutoMapper;
using BlockchainAuthIoT.DataProvider.Attributes;
using BlockchainAuthIoT.DataProvider.Entities;
using BlockchainAuthIoT.DataProvider.Repositories;
using BlockchainAuthIoT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IMapper _mapper;

        public TemperatureController(ILogger<TemperatureController> logger, ITemperatureRepository repo)
        {
            _logger = logger;
            _repo = repo;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<TemperatureEntity, TemperatureReading>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        [ValidateIntParam("count", IntCondition.LessOrEqualTo, "max_items")]
        [ValidateListParam("deviceNames", ListCondition.AllContainedIn, "devices")]
        public async Task<ObjectResult> Latest(int count = 1, string deviceNames = "")
        {
            var entities = await _repo.GetLatest(count, deviceNames.Split(','));
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return new ObjectResult(entities.Select(e => _mapper.Map<TemperatureReading>(e)));
        }
    }
}
