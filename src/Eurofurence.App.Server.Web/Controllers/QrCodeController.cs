using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("qr")]
    [Route("qrcode")]
    public class QrCodeController : BaseController
    {
        private readonly IQrCodeService _qrCodeService;

        public QrCodeController(
            IQrCodeService qrCodeService
            )
        {
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Offers link-shortening capabilities for use in printed QR codes to allow changing their
        /// targets even after they have been printed (e.g. in case of URLs changing at a later point).
        /// </summary>
        /// <param name="targetId">Short name for the redirection target.</param>
        /// <returns>HTTP redirect to the target specified by <c>targetId</c>.</returns>
        [HttpGet("{targetId}")]
        public ActionResult GetTargetRedirect(string targetId)
        {
            try
            {
                return new RedirectResult(_qrCodeService.GetTarget(targetId));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}