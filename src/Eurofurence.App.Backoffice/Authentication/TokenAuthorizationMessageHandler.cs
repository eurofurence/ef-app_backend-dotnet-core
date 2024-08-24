using System.Security.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

public class TokenAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly NavigationManager _navigation;

    public TokenAuthorizationMessageHandler(IAccessTokenProvider tokenProvider, NavigationManager navigation)
    {
        _tokenProvider = tokenProvider;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var result = await _tokenProvider.RequestAccessToken();

        if (result.TryGetToken(out var token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);
            return await base.SendAsync(request, cancellationToken);
        }
        else
        {
            // Redirect to login page if the token is not available
            if (!_navigation.Uri.Contains("authentication/login"))
            {
                _navigation.NavigateTo("authentication/login");
            }
            throw new AccessTokenNotAvailableException(_navigation, result, null);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
                throw new AuthenticationException("Authentication has expired. Redirecting to login...");
            }
        }
    }
}