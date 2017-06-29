using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysAlternativePinRecord : EntityBase
    {
        public class RequestRecord
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

        public IList<RequestRecord> RequestLog { get; set; }
        public IList<DateTime> PinConsumptionDatesUtc { get; set; }

        public RegSysAlternativePinRecord()
        {
            PinConsumptionDatesUtc = new List<DateTime>();
            RequestLog = new List<RequestRecord>();
        }
    }
}
