namespace Eurofurence.App.Server.Web.Identity;

public class IdentityOptions
{
    public string ClientId { get; set; }
    public string IntrospectionEndpoint { get; set; }
    public string UserInfoEndpoint { get; set; }
    public string RegSysUrl { get; set; }
}