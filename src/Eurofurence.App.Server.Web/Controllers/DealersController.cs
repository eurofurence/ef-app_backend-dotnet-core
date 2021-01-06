using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class DealersController : BaseController
    {
        private readonly IDealerService _dealerService;
        private readonly IImageService _imageService;

        public DealersController(
            IDealerService dealerService,
            IImageService imageService)
        {
            _dealerService = dealerService;
            _imageService = imageService;
        }

        /// <summary>
        ///     Retrieves a list of all dealer entries.
        /// </summary>
        /// <returns>All dealer Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<DealerRecord>), 200)]
        public Task<IEnumerable<DealerRecord>> GetDealerEntriesAsync()
        {
            return _dealerService.FindAllAsync();
        }

        /// <summary>
        ///     Retrieve a single dealer.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DealerRecord), 200)]
        public async Task<DealerRecord> GetDealerAsync([FromRoute] Guid id)
        {
            return (await _dealerService.FindOneAsync(id)).Transient404(HttpContext);
        }


        /// <summary>
        ///     Update an existing dealer.
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutDealerAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("id")] DealerRecord Record,
            [EnsureNotNull][FromRoute] Guid id)
        {
            var exists = await _dealerService.HasOneAsync(id);
            if (!exists) return NotFound($"No record found with it {id}");

            Record.Touch();

            var imageIdsExist = await _imageService.HasManyAsync(
                Record.ArtistImageId, Record.ArtPreviewImageId, Record.ArtistThumbnailImageId
                );

            if (!imageIdsExist) return BadRequest($"Invalid image ids specified");

            await _dealerService.ReplaceOneAsync(Record);

            return NoContent();
        }

        /// <summary>
        ///     Create a new dealer.
        /// </summary>
        /// <param name="Record"></param>
        /// <returns>Id of the newly created dealer</returns>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(400)]
        [HttpPost("")]
        public async Task<ActionResult> PostDealerAsync(
            [EnsureNotNull][FromBody] DealerRecord Record
        )
        {
            Record.NewId();
            Record.Touch();

            var imageIdsExist = await _imageService.HasManyAsync(
                Record.ArtistImageId, Record.ArtPreviewImageId, Record.ArtistThumbnailImageId
                );

            if (!imageIdsExist) return BadRequest($"Invalid image ids specified");

            await _dealerService.InsertOneAsync(Record);

            return Ok(Record.Id);
        }

        /// <summary>
        ///     Delete a dealer.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> DeleteDealerAsync([EnsureNotNull][FromRoute] Guid id)
        {
            var exists = await _dealerService.HasOneAsync(id);
            if (!exists) return NotFound();

            await _dealerService.DeleteOneAsync(id);

            return NoContent();
        }


    }
}