using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class TestController
    {
        /// <summary>
        /// Returns a simple string.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Public")]
        public string GetPublicString()
        {
            return "This is public.";
        }

        /// <summary>
        /// Returns a simple string, but requires authentication.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Protected")]
        [Authorize("OAuth-AllAuthenticated")]
        public string GetProtectedString()
        {
            return "This should only be accessible with authentication.";
        }
    }
}
