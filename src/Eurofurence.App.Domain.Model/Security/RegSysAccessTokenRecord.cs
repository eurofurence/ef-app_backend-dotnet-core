using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysAccessTokenRecord : EntityBase
    {
        public string Token { get; set; }
        public List<RoleRecord> GrantRoles { get; set; } = new();

        public string ClaimedByUid { get; set; }
        public DateTime? ClaimedAtDateTimeUtc { get; set; }
    }
}