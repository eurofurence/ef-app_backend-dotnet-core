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