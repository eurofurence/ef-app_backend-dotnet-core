using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class TokensController : BaseController
    {
        private readonly IApiPrincipal _apiPrincipal;
        private readonly IAuthenticationHandler _authenticationHandler;
        private readonly IRegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;

        public TokensController(IAuthenticationHandler authenticationHandler,
            IRegSysAlternativePinAuthenticationProvider regSysAlternativePinAuthenticationProvider,
            IApiPrincipal apiPrincipal)
        {
            _authenticationHandler = authenticationHandler;
            _regSysAlternativePinAuthenticationProvider = regSysAlternativePinAuthenticationProvider;
            _apiPrincipal = apiPrincipal;
        }

        [HttpPost("RegSys")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        [ProducesResponseType(403)]
        public async Task<AuthenticationResponse> PostRegSysAuthenticationRequest(
            [EnsureNotNull][FromBody] RegSysAuthenticationRequest request)
        {
            return (await _authenticationHandler.AuthorizeViaRegSys(request))
                .Transient403(HttpContext);
        }

        [HttpGet("WhoAmI")]
        [Authorize("OAuth-AllAuthenticated")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        [ProducesResponseType(403)]
        public AuthenticationResponse GetWhoAmIInfo()
        {
            return _authenticationHandler.AuthorizeViaPrincipal(_apiPrincipal);
        }
    }
}