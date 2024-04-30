using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysIdentityRecord : EntityBase
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public List<RoleRecord> Roles { get; set; } = new();
    }
}
