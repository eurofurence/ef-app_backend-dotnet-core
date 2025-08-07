namespace Eurofurence.App.Server.Services.Abstractions.Identity;

public class IdentityOptions
{
    public string ClientId { get; init; }
    public string IntrospectionEndpoint { get; init; }
    public string UserInfoEndpoint { get; init; }
    public string GroupsEndpoint { get; set; }
    public string RegSysUrl { get; init; }
    public string GroupReaderToken { get; init; }
    public int GroupCacheExpirationInHours { get; init; }
}