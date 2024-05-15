using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysAlternativePinRecord : EntityBase
    {
        public int RegNo { get; set; }
        public string Pin { get; set; }
        public string NameOnBadge { get; set; }

        public DateTime IssuedDateTimeUtc { get; set; }
        public string IssuedByUid { get; set; }

        public List<IssueRecord> IssueLog { get; set; } = new();
        public List<DateTime> PinConsumptionDatesUtc { get; set; } = new();
    }
}
