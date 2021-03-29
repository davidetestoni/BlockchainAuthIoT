using AutoMapper;
using BlockchainAuthIoT.DataProvider.Entities;
using BlockchainAuthIoT.DataProvider.Repositories;
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
        private readonly IMapper _mapper;

        public DataController(ILogger<DataController> logger, IDataRepository repo)
        {
            _logger = logger;
            _repo = repo;

            var config = new MapperConfiguration(cfg => cfg.CreateMap<DataEntity, SampleData>());
            _mapper = new Mapper(config);
        }

        [HttpGet]
        public async Task<IEnumerable<SampleData>> GetAll()
        {
            var entities = await _repo.GetAll().ToListAsync();
            _logger.LogInformation($"Got {entities.Count} entities from the database");
            return entities.Select(e => _mapper.Map<SampleData>(e));
        }
    }
}
