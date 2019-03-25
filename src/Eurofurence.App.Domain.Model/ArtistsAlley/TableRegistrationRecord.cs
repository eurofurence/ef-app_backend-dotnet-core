using Eurofurence.App.Domain.Model.Fragments;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationRecord : EntityBase
    {
        public string OwnerUid { get; set; }

        public string DisplayName { get; set; }

        public string Merchandise { get; set; }

        public string ShortDescription { get; set; }

        public ImageFragment Image { get; set; }
    }
}
