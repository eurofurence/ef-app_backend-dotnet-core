using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    /// <summary>
    /// A global status of the artist alley system
    /// </summary>
    public enum ArtistAlleySystemStatus
    {
        /// <summary>
        /// The artist alley is normal open
        /// </summary>
        OPEN = 0,

        /// <summary>
        /// The registrations for tables at the artist alley are temporarily closed
        /// </summary>
        REG_CLOSED = 1,
    }
}