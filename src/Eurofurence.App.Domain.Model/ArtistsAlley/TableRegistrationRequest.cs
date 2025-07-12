using System.ComponentModel.DataAnnotations;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationRequest : IDtoTransformable<TableRegistrationRecord>
    {
        /// <summary>
        /// Preferred display name of artist.
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional URL of artist's website.
        /// </summary>
        public string WebsiteUrl { get; set; }

        /// <summary>
        /// Short text provided by the artist on who they are and what they are offering.
        /// </summary>
        [Required]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Table number at which the artist has seated themselves in the Artist Alley.
        /// Must be > 0 and may have optional, upper limit.
        /// </summary>
        [Required]
        public string Location { get; set; }

        /// <summary>
        /// Optional Telegram handle (prefixed @ will be removed automatically).
        /// </summary>
        public string TelegramHandle { get; set; }
    }
}
