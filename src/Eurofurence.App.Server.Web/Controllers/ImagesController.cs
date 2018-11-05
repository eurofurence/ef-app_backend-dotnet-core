using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ImagesController : BaseController
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        ///     Retrieves a list of all images.
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
        ///     Retrieve a single image.
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
        ///     Retrieve a single image content.
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
        ///     Retrieve a single image content using hash code (preferred, as it allows caching).
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        /// <param name="contentHashBase64Encoded">Base64 Encoded ContentHashSha1 of the requested entity</param>
        [HttpGet("{Id}/Content/with-hash:{contentHashBase64Encoded}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(byte[]), 200)]
        [ResponseCache(Duration = 60 * 60 * 24, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> GetImageWithHashContentAsync(
            [EnsureNotNull][FromRoute] Guid id,
            [EnsureNotNull][FromRoute] string contentHashBase64Encoded)
        {
            var record = await _imageService.FindOneAsync(id);
            if (record == null) return NotFound();

            var contentHash =
                Encoding.Default.GetString(Convert.FromBase64String(contentHashBase64Encoded));

            if (record.ContentHashSha1 != contentHash)
            {
                return Redirect($"./with-hash:{Convert.ToBase64String(Encoding.Default.GetBytes(record.ContentHashSha1))}");
            }

            var content = await _imageService.GetImageContentByIdAsync(id);
            return File(content, record.MimeType);
        }

        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [HttpPut("{Id}/Content")]
        public async Task<ActionResult> PutImageContentAsync([FromRoute] Guid id, [FromBody] string ImageContent)
        {
            var record = await _imageService.FindOneAsync(id);
            if (record == null) return NotFound();

            byte[] imageBytes = Convert.FromBase64String(ImageContent);

            await _imageService.InsertOrUpdateImageAsync(record.InternalReference, imageBytes);

            return NoContent();            
        }

    }
}