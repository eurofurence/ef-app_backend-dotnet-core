using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class LostAndFoundController : BaseController
    {
        private readonly ILostAndFoundService _lostAndFoundService;
        private readonly IApiPrincipal _apiPrincipal;

        public LostAndFoundController(
            ILostAndFoundService lostAndFoundService,
            IApiPrincipal apiPrincipal
            )
        {
            _lostAndFoundService = lostAndFoundService;
            _apiPrincipal = apiPrincipal;
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("Items")]
        public  IQueryable<LostAndFoundRecord> GetItemsAsync()
        {
            return _lostAndFoundService.FindAll();
        }
    }
} 