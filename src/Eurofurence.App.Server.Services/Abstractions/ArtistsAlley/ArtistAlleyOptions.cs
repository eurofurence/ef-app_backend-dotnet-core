namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class ArtistAlleyOptions
    {
        public bool RegistrationEnabled { get; init; }
        public bool SendAnnouncements { get; init; }
        public double? ExpirationTimeInHours { get; init; }
    }
}
