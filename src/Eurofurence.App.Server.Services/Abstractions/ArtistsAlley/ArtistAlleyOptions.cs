namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class ArtistAlleyOptions
    {
        public bool RegistrationEnabled { get; init; }
        public ArtistAlleyTelegramOptions Telegram { get; init; }
    }
}
