using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Security
{
    public class RegSysIdentityRecord : EntityBase
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; }

        public RegSysIdentityRecord()
        {
            Roles = new List<string>();
        }
    }
}
