using Eurofurence.App.Server.Services.Abstractions.AppConfig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class AppConfigController : BaseController
    {
        private readonly IAppConfigService _appConfigService;

        public AppConfigController(
            IAppConfigService appConfigService
            )
        {
            _appConfigService = appConfigService;
        }

        /// <summary>
        /// Dynamic configuration that is fetched by the app to allow reconfiguration of certain
        /// options without having to republish the app.
        /// </summary>
        /// <returns>
        ///     Dictionary with key value pairs for feature flags and predefined configuration options.
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        public ActionResult GetAppConfig()
        {
            return Ok(_appConfigService.Get());
        }
    }
}