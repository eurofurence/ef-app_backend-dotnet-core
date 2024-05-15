using System.Linq;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class LostAndFoundController : BaseController
    {
        private readonly ILostAndFoundService _lostAndFoundService;

        public LostAndFoundController(ILostAndFoundService lostAndFoundService)
        {
            _lostAndFoundService = lostAndFoundService;
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("Items")]
        public  IQueryable<LostAndFoundRecord> GetItemsAsync()
        {
            return _lostAndFoundService.FindAll();
        }
    }
} 