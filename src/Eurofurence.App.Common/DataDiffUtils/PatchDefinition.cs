using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Common.DataDiffUtils
{
    public class PatchDefinition<TSource, TTarget> where TTarget : IEntityBase, new()
    {
        private readonly List<PatchOperator<TSource, TTarget>> _operators = new List<PatchOperator<TSource, TTarget>>();
        private readonly Func<TSource, IEnumerable<TTarget>, TTarget> _targetItemLocator;

        /// <param name="targetItemLocator">
        ///     A selector that, inside an enumerable of TTarget, finds the
        ///     single TTarget that corresponds to the provided TSource
        /// </param>
        public PatchDefinition(Func<TSource, IEnumerable<TTarget>, TTarget> targetItemLocator)
        {
            _targetItemLocator = targetItemLocator;
        }

        public PatchDefinition<TSource, TTarget> Map<TField>(
            Func<TSource, TField> sourceValueSelector,
            Expression<Func<TTarget, TField>> targetSelector)
        {
            var patchOperator = new PatchOperator<TSource, TTarget>
            {
                FieldName = (targetSelector.Body as MemberExpression)?.Member.Name,
                IsEqual = (source, target) =>
                {
                    var sourceValue = sourceValueSelector(source);
                    var targetValue = targetSelector.Compile().Invoke(target);

                    if (sourceValue == null && targetValue == null)
                        return true;

                    if (sourceValue == null && targetValue != null || sourceValue != null && targetValue == null)
                        return false;

                    if (sourceValue.GetType().IsArray)
                        return (sourceValue as IEnumerable<object>).SequenceEqual(targetValue as IEnumerable<object>
                        );

                    return sourceValue?.Equals(targetValue) ?? false;
                },
                ApplySourceValueToTarget =
                    (source, target) =>
                    {
                        var value = sourceValueSelector(source);
                        var memberSelectorExpression = targetSelector.Body as MemberExpression;
                        var property = memberSelectorExpression.Member as PropertyInfo;
                        property.SetValue(target, value, null);
                    }
            };

            _operators.Add(patchOperator);

            return this;
        }


        public List<PatchOperation<TTarget>> Patch(IEnumerable<TSource> sources, IEnumerable<TTarget> targets)
        {
            var patchResults = new List<PatchOperation<TTarget>>();

            var unprocessedTargets = targets.ToList();
            var newTargets = new List<TTarget>();

            foreach (var sourceItem in sources)
            {
                var target = default(TTarget);

                var result = new PatchOperation<TTarget> {Action = ActionEnum.NotModified};

                var existingTarget = _targetItemLocator(sourceItem, unprocessedTargets);
                if (existingTarget != null)
                {
                    target = existingTarget;
                    unprocessedTargets.Remove(target);
                }
                else
                {
                    target = new TTarget();
                    target.NewId();
                    newTargets.Add(target);
                    result.Action = ActionEnum.Add;
                }

                result.Entity = target;

                foreach (var o in _operators)
                {
                    if (o.IsEqual(sourceItem, target)) continue;

                    if (result.Action == ActionEnum.NotModified)
                        result.Action = ActionEnum.Update;

                    o.ApplySourceValueToTarget(sourceItem, target);
                }

                patchResults.Add(result);
            }

            foreach (var targetItem in unprocessedTargets)
                patchResults.Add(new PatchOperation<TTarget>
                {
                    Action = ActionEnum.Delete,
                    Entity = targetItem
                });

            return patchResults;
        }
    }
}