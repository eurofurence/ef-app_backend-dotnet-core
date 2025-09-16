using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("qr")]
    [Route("qrcode")]
    //FIXME: Temporary solution for EF29
    [Route("wifi")]
    public class QrCodeController : BaseController
    {
        private readonly IQrCodeService _qrCodeService;

        public QrCodeController(
            IQrCodeService qrCodeService
            )
        {
            _qrCodeService = qrCodeService;
        }

        //FIXME: Temporary solution for EF29
        [HttpGet("")]
        public ActionResult GetTargetRedirect()
        {
            return GetTargetRedirect("wifi");
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