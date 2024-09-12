namespace Eurofurence.App.Server.Web.Identity;

public class ApiKeyAuthenticationDefaults
{

    /// <summary>
    /// The default authentication scheme.
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// Header name for the API keys.
    /// </summary>
    public const string HeaderName = "X-API-Key";
}