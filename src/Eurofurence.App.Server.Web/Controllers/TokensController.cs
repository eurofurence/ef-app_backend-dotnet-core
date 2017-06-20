using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class TokensController : Controller
    {
        private readonly ApiPrincipal _apiPrincipal;
        private readonly IAuthenticationHandler _authenticationHandler;

        public TokensController(IAuthenticationHandler authenticationHandler, ApiPrincipal apiPrincipal)
        {
            _authenticationHandler = authenticationHandler;
            _apiPrincipal = apiPrincipal;
        }

        [HttpPost("RegSys")]
        [ProducesResponseType(typeof(AuthenticationResponse), 200)]
        [ProducesResponseType(403)]
        public async Task<AuthenticationResponse> PostRegSysAuthenticationRequest(
            [FromBody] RegSysAuthenticationRequest request)
        {
            return (await _authenticationHandler.AuthorizeViaRegSys(request))
                .Transient403(HttpContext);
        }


        [HttpGet("WhoAmI")]
        [Authorize("OAuth-AllAuthenticated")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(403)]
        public string GetWhoAmIInfo()
        {
            var message = new List<string>();

            message.Add("What your authorization tells me about you:");

            foreach (var claim in _apiPrincipal.Claims)
                message.Add($"{claim.Key} = {claim.Value}");

            return string.Join("\n", message.ToArray());
        }
    }
}