using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    [AllowAnonymous]
    public class DealersController : BaseController
    {
        private readonly IDealerService _dealerService;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public DealersController(
            IDealerService dealerService,
            IImageService imageService,
            IMapper mapper)
        {
            _dealerService = dealerService;
            _imageService = imageService;
            _mapper = mapper;
        }

        /// <summary>
        ///     Retrieves a list of all dealer entries.
        /// </summary>
        /// <returns>All dealer Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<DealerRecord>), 200)]
        public IQueryable<DealerRecord> GetDealerEntriesAsync()
        {
            return _dealerService.FindAll();
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
        /// <param name="request"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutDealerAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("id")] DealerRequest request,
            [EnsureNotNull][FromRoute] Guid id)
        {
            var exists = await _dealerService.HasOneAsync(id);
            if (!exists) return NotFound($"No record found with it {id}");

            request.Touch();

            var imageIdsExist = await _imageService.HasManyAsync(
                request.ArtistImageId, request.ArtPreviewImageId, request.ArtistThumbnailImageId
                );

            if (!imageIdsExist) return BadRequest($"Invalid image ids specified");

            var record = _mapper.Map<DealerRecord>(request);
            await _dealerService.ReplaceOneAsync(record);

            return NoContent();
        }

        /// <summary>
        ///     Create a new dealer.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Id of the newly created dealer</returns>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(400)]
        [HttpPost("")]
        public async Task<ActionResult> PostDealerAsync(
            [EnsureNotNull][FromBody] DealerRequest request
        )
        {
            request.NewId();
            request.Touch();

            var imageIdsExist = await _imageService.HasManyAsync(
                request.ArtistImageId, request.ArtPreviewImageId, request.ArtistThumbnailImageId
                );

            if (!imageIdsExist) return BadRequest($"Invalid image ids specified");

            var record = _mapper.Map<DealerRecord>(request);
            await _dealerService.InsertOneAsync(record);

            return Ok(record.Id);
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