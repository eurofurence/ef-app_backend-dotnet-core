namespace Eurofurence.App.Server.Services.Abstractions.Identity;

public class IdentityOptions
{
    public string ClientId { get; init; }
    public string IntrospectionEndpoint { get; init; }
    public string UserInfoEndpoint { get; init; }
    public string RegSysUrl { get; init; }
}