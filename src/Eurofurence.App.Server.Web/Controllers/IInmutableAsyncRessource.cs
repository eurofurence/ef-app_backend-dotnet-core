using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers;

public interface IInmutableAsyncRessource<TRecord, TDTO>
// where TRecord : IDtoTransformer<TDTO>
// where TDTO : IDtoTransformer<TRecord>
{
    Task<ActionResult<IEnumerable<TDTO>>> GetAllAsync();

    Task<ActionResult<TDTO>> GetByIdAsync(Guid id);

    Task<ActionResult> CreateAsync(TDTO record);
}