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
    public class HumidityController : ControllerBase
    {
        private readonly ILogger<HumidityController> _logger;
        private readonly IHumidityRepository _repo;
        private readonly IMapper _mapper;

        public HumidityController(ILogger<HumidityController> logger, IHumidityRepository repo)
        {
            _logger = logger;
            _repo = repo;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<HumidityEntity, HumidityReading>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        [ValidateIntParam("count", IntCondition.LessOrEqualTo, "max_items")]
        [ValidateListParam("deviceNames", ListCondition.AllContainedIn, "devices")]
        public async Task<ObjectResult> Latest(int count = 1, string deviceNames = "")
        {
            var entities = await _repo.GetLatest(count, deviceNames.Split(','));
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return new ObjectResult(entities.Select(e => _mapper.Map<HumidityReading>(e)));
        }
    }
}
