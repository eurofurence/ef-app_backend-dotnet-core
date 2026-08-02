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
        /// <para>Dynamic configuration that is fetched by the app to allow reconfiguration of certain
        /// options without having to republish the app.</para>
        /// 
        /// Feature flag suffixes work as follows:
        /// <list>
        ///     <item>
        ///         <term>…Enabled</term>
        ///         <description>
        ///             Feature is disabled in app if flag is not provided.
        ///             Value <c>true</c> will enable the feature.
        ///             Value <c>false</c> is the assumed default.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>…Disabled</term>
        ///         <description>
        ///             Feature is enabled in app if flag is not provided.
        ///             Value <c>true</c> will disable the feature.
        ///             Value <c>false</c> is the assumed default.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>No Suffix on Feature Flag</term>
        ///         <description>
        ///             App defaults to state not explicitly known to or defined by the backend.
        ///             Value can be any <c>string</c>-y expression e.g.
        ///             <c>true</c>, <c>621</c> or <c>"foobar"</c>.
        ///         </description>
        ///     </item>
        /// </list>
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