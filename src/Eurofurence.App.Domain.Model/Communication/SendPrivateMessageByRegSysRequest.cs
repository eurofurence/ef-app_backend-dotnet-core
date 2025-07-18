﻿using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.Communication
{
    public class SendPrivateMessageByRegSysRequest : IDtoTransformable<PrivateMessageRecord>
    {
        public string RecipientUid { get; set; }
        public string AuthorName { get; set; }
        public string ToastTitle { get; set; }
        public string ToastMessage { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}