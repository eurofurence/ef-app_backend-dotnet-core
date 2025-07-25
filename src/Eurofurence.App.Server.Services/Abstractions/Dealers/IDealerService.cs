﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Server.Services.Abstractions.Dealers
{
    public interface IDealerService :
        IEntityServiceOperations<DealerRecord, DealerResponse>,
        IPatchOperationProcessor<DealerRecord>
    {
        public Task RunImportAsync(CancellationToken cancellationToken = default);

        public string GetMapLink(Guid id);
    }
}