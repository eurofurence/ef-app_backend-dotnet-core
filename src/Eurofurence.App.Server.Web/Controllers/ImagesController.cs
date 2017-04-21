using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ImagesController : Controller
    {
        readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// Retrieves a list of all images.
        /// </summary>
        /// <returns>All knowledge groups.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<ImageRecord>), 200)]
        public Task<IEnumerable<ImageRecord>> GetImagesAsync()
        {
            return _imageService.FindAllAsync();
        }

        /// <summary>
        /// Retrieve a single image.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(ImageRecord), 200)]
        public async Task<ImageRecord> GetImageAsync([FromRoute] Guid id)
        {
            return (await _imageService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        /// Retrieve a single image content.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}/Content")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(byte[]), 200)]
        public async Task<ActionResult> GetImageContentAsync([FromRoute] Guid id)
        {
            var record = await _imageService.FindOneAsync(id);
            if (record == null) return NotFound();

            var content = await _imageService.GetImageContentByIdAsync(id);
            return File(content, record.MimeType);
        }

        /// <summary>
        /// Retrieves a delta of images since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<ImageRecord>), 200)]
        public Task<DeltaResponse<ImageRecord>> GetImagesDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _imageService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
