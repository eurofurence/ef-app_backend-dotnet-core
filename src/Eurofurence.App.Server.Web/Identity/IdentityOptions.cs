namespace Eurofurence.App.Server.Web.Identity;

public record IdentityOptions(string ClientId, string IntrospectionEndpoint, string UserInfoEndpoint);