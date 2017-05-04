using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public Task<AuthenticationResponse> PostRegSysAuthenticationRequest([FromBody] RegSysAuthenticationRequest request)
        {
            return  _authenticationHandler.AuthorizeViaRegSys(request);
        }
    }
}
