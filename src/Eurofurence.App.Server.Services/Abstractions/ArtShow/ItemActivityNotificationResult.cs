using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public class ItemActivityNotificationResult
    {
        public string RecipientUid { get; set; }
        public IList<int> IdsSold { get; set; }
        public IList<int> IdsToAuction { get; set; }
        public int GrandTotal { get; set; }
    }
}