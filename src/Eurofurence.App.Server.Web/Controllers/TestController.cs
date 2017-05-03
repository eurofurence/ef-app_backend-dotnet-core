using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class TestController : Controller
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
            var message = new List<string>();
            message.Add("This should only be accessible with authentication.\n");

            var claimsIdentity = (HttpContext.User.Identity as ClaimsIdentity);
            if (claimsIdentity != null)
            {
                message.Add("What your authorization tells me about you:");

                foreach (var claim in claimsIdentity.Claims)
                {
                    message.Add($"{claim.Type} = {claim.Value} ({claim.ValueType})");
                }
            }

            return String.Join("\n", message.ToArray());
        }
    }
}
