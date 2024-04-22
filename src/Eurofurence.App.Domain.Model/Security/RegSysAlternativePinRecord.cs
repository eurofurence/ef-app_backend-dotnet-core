using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysAlternativePinRecord : EntityBase
    {
        public class IssueRecord : EntityBase
        {
            public string RequesterUid { get; set; }
            public string NameOnBadge { get; set; }
            public DateTime RequestDateTimeUtc { get; set; }
        }

        public int RegNo { get; set; }
        public string Pin { get; set; }
        public string NameOnBadge { get; set; }

        public DateTime IssuedDateTimeUtc { get; set; }
        public string IssuedByUid { get; set; }

        public IList<IssueRecord> IssueLog { get; set; } = new List<IssueRecord>();
        public IList<DateTime> PinConsumptionDatesUtc { get; set; } = new List<DateTime>();
    }
}
