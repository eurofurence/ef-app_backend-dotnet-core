using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Common.DataDiffUtils
{
    public class PatchOperation<T> where T : IEntityBase
    {
        public T Entity { get; set; }

        public ActionEnum Action { get; set; }
    }
}