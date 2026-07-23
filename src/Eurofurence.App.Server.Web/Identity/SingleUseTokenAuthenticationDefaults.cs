namespace Eurofurence.App.Server.Web.Identity;

public class SingleUseTokenAuthenticationDefaults
{
    /// <summary>
    /// The default authentication scheme.
    /// </summary>
    public const string AuthenticationScheme = "SingleUseToken";
    /// <summary>
    /// Authentication scheme that can be used to fall back to OAuth2 if no token is provided.
    /// </summary>
    public const string TokenOrOAuth2AuthenticationPolicyScheme = "SingleUseTokenOrOAuth2";

    /// <summary>
    /// Query field name for the single-use token.
    /// </summary>
    public const string QueryName = "token";
}