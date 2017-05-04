using System.Security.Claims;

namespace Eurofurence.App.Server.Services.Security
{
    public class ApiPrincipal
    {
        readonly ClaimsPrincipal _principal;

        public ApiPrincipal(ClaimsPrincipal principal)
        {
            _principal = principal;
        }

        public bool IsAttendee => _principal?.IsInRole("Attendee") ?? false;
        public string Uid => _principal?.Identity.Name ?? "Anonymous";
    }
}
