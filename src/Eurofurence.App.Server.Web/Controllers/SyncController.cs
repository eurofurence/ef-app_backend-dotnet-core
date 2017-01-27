using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class SyncController
    {
        /// <summary>
        /// Returns a simple string.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> GetDeltaAsync(DateTime? since = null)
        {
            return "This is public.";
        }
    }
}
