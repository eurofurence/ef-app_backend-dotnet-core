using System;

namespace Eurofurence.App.Domain.Model.Security
{
    public class IssueRecord : EntityBase
    {
        public string RequesterUid { get; set; }
        public string NameOnBadge { get; set; }
        public DateTime RequestDateTimeUtc { get; set; }
    }
}
