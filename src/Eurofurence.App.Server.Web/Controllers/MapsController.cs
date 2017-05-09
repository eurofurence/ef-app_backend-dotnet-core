using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Maps;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class MapsController : Controller
    {
        readonly IMapService _mapService;

        public MapsController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<MapRecord>), 200)]
        public Task<IEnumerable<MapRecord>> GetMapsAsync()
        {
            return _mapService.FindAllAsync();
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(MapRecord), 200)]
        public async Task<MapRecord> GetMapAsync([FromRoute] Guid id)
        {
            return (await _mapService.FindOneAsync(id)).Transient404(HttpContext);
        }


        [HttpGet("{Id}/Entries")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ICollection<MapEntryRecord>), 200)]
        public async Task<ICollection<MapEntryRecord>> GetMapEntriesAsync([FromRoute] Guid id)
        {
            return (await _mapService.FindOneAsync(id)).Transient404(HttpContext)?.Entries;
        }
    }
}
