using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Eurofurence.App.Server.Web.Identity;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public IList<ApiKeyOptions> ApiKeys { get; set; }
    public class ApiKeyOptions {
        public string Key { get; set;}
        public string PrincipalName { get; set;}
        public DateTime ValidUntil { get; set;}
        public IList<string> Roles { get; set;}
    }
}
