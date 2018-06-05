using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysAccessTokenRecord : EntityBase
    {
        public string Token { get; set; }
        public IList<string> GrantRoles { get; set; }

        public string ClaimedByUid { get; set; }
        public DateTime? ClaimedAtDateTimeUtc { get; set; }

        public RegSysAccessTokenRecord()
        {
            GrantRoles = new List<string>();
        }
    }
}