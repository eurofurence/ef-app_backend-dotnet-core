using System;

namespace Eurofurence.App.Common.DataDiffUtils
{
    public class PatchOperator<TSource, TTarget>
    {
        public string FieldName { get; set; }
        public object NewValue { get; set; }
        public Func<TSource, TTarget, bool> IsEqual { get; set; }
        public Action<TSource, TTarget> ApplySourceValueToTarget { get; set; }
    }
}