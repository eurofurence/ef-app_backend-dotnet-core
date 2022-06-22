using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Server.Services.Abstractions.ArtShow
{
    public class ImportResult
    {
        public int RowsImported { get; set; }
        public int RowsSkippedAsDuplicate { get; set; }
    }
}
