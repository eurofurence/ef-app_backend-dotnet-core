#nullable enable
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
        /// <response code="200">
        /// Feature flags are named `Feature{Name}{Suffix}` with suffixes as follows:
        /// - **…Enabled:**
        ///   Feature is disabled in app if flag is not explicitly provided.
        ///   Value <c>true</c> will enable the feature.
        ///   Value <c>false</c> is the assumed default.
        /// - **…Disabled:**
        ///   Feature is enabled in app if flag is not explicitly provided.
        ///   Value <c>true</c> will disable the feature.
        ///   Value <c>false</c> is the assumed default.
        /// - **No suffix on feature flag:**
        ///   Dynamic feature configuration; value can be any <c>string</c>-y expression e.g.
        ///   <c>true</c>, <c>621</c> or <c>"foobar"</c>.
        /// </response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        public ActionResult GetAppConfig()
        {
            return Ok(_appConfigService.Get());
        }
    }
}