using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Common.Validation;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class TokensController : Controller
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
            [FromBody] RegSysAuthenticationRequest request)
        {
            return (await _authenticationHandler.AuthorizeViaRegSys(request))
                .Transient403(HttpContext);
        }


        [Authorize(Roles = "Developer,System,Security,ConOps,Registration")]
        [HttpPost("RegSys/AlternativePin")]
        [ProducesResponseType(typeof(RegSysAlternativePinResponse), 200)]
        public async Task<ActionResult> PostRegSysAlternativePinRequest(
            [FromBody] RegSysAlternativePinRequest request)
        {
            if (request == null) return BadRequest("Unable to parse request");
            if (!BadgeChecksum.IsValid(request.RegNoOnBadge)) return BadRequest("Invalid Badge No.");

            var result =  await _regSysAlternativePinAuthenticationProvider
                .RequestAlternativePinAsync(request, _apiPrincipal.Uid);

            return Json(result);
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