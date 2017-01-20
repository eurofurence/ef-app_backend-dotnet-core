using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Server.Services.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using MongoDB.Bson.Serialization;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Annotations;

namespace Eurofurence.App.Server.Web.Controllers
{

    [Route("Api/[controller]")]
    public class TestController : Controller
    {
        private readonly IEventService _eventService;

        public TestController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("TestString")]
        public IActionResult TestString()
        {
            return Content("");
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventRecord[]), 200)]
        public Task<IEnumerable<EventRecord>> GetEventsAsync()
        {
            return _eventService.FindAllAsync();
        }


    }
}
