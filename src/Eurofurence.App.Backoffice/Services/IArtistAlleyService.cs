using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IArtistAlleyService
    {
        /// <summary>
        /// Send a query to the api and retrieve all registrations for the artist alley
        /// </summary>
        /// <returns>The list of found database entries</returns>
        public Task<TableRegistrationRecord[]> GetTableRegistrationsAsync();

        /// <summary>
        /// Changes the status of a given artist alley registration <paramref name="record"/>
        /// </summary>
        /// <param name="record">The record that should be updated</param>
        /// <param name="state"></param>
        /// <returns></returns>
        public Task PutTableRegistrationStatusAsync(TableRegistrationRecord record, TableRegistrationRecord.RegistrationStateEnum state);

        /// <summary>
        /// Deletes a given <see cref="TableRegistrationRecord"/>
        /// </summary>
        /// <param name="record">The record to delete</param>
        /// <returns></returns>
        public Task DeleteTableRegistrationAsync(TableRegistrationRecord record);

        /// <summary>
        /// Sets the global status for the artist alley system.
        ///
        /// </summary>
        /// <param name="status">The new status</param>
        /// <returns></returns>
        public Task PutArtistAllaySystemStatus(ArtistAlleySystemStatus status);

        /// <summary>
        /// Returns the current global status of the artist alley system
        /// </summary>
        /// <returns></returns>
        public Task<ArtistAlleySystemStatus> GetArtistAllaySystemStatus();
    }
}