using System;
using Eurofurence.App.Common.Utility;

namespace Eurofurence.App.Server.Services.ArtShow
{
    class LogImportRow
    {
        public int RegNo { get; set; }
        public int ASIDNO { get; set; }
        public string ArtistName { get; set; }
        public string ArtPieceTitle { get; set; }
        public string Status { get; set; }
        public int? FinalBidAmount { get; set; }

        public Lazy<string> Hash => new Lazy<string>(
            () => Hashing.ComputeHashSha1(RegNo, ASIDNO, ArtistName, ArtPieceTitle, Status, FinalBidAmount ?? -1));
    }
}