using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Eurofurence.App.Server.Web.Extensions;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class TokensController : Controller
    {
        readonly IAuthenticationHandler _authenticationHandler;

        public TokensController(IAuthenticationHandler authenticationHandler)
        {
            _authenticationHandler = authenticationHandler;
        }

        [HttpPost("RegSys")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        [ProducesResponseType(403)]
        public async Task<AuthenticationResponse> PostRegSysAuthenticationRequest([FromBody] RegSysAuthenticationRequest request)
        {
            return (await _authenticationHandler.AuthorizeViaRegSys(request))
                .Transient403(HttpContext);
        }
    }
}
