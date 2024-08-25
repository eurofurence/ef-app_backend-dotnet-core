using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Common.DataDiffUtils
{
    public class PatchDefinition<TSource, TTarget> where TTarget : IEntityBase, new()
    {
        private readonly List<PatchOperator<TSource, TTarget>> _operators = [];
        private readonly Func<TSource, IEnumerable<TTarget>, TTarget> _targetItemLocator;

        /// <param name="targetItemLocator">
        ///     A selector that, inside an IEnumerable of TTarget, finds the
        ///     single TTarget that corresponds to the provided TSource
        /// </param>
        public PatchDefinition(Func<TSource, IEnumerable<TTarget>, TTarget> targetItemLocator)
        {
            _targetItemLocator = targetItemLocator;
        }

        private static bool ArraysEqual(Array a1, Array a2)
        {
            if (a1.Length == a2.Length)
            {
                var a1e = a1.GetEnumerator();
                var a2e = a2.GetEnumerator();

                for (int i = 0; i < a1.Length; i++)
                {
                    if (!a1e.MoveNext() || !a2e.MoveNext() || !a1e.Current.Equals(a2e.Current))
                        return false;
                }
                return true;
            }
            return false;
        }

        private static bool CollectionsEqual(ICollection collection1, ICollection collection2)
        {
            if (collection1.Count == collection2.Count)
            {
                var a1e = collection1.GetEnumerator();
                var a2e = collection2.GetEnumerator();

                for (int i = 0; i < collection1.Count; i++)
                {
                    if (!a1e.MoveNext() || !a2e.MoveNext())
                    {
                        return false;
                    }

                    if (!a1e.Current.Equals(a2e.Current))
                        return false;
                }
                return true;
            }
            return false;
        }

        private static bool DictionariesEqual(IDictionary dictionary1, IDictionary dictionary2)
        {
            var string1 = JsonSerializer.Serialize(dictionary1);
            var string2 = JsonSerializer.Serialize(dictionary2);
            return string1 == string2;
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

                    if (sourceValue == null || targetValue == null)
                        return false;

                    if (sourceValue.GetType().IsArray)
                        return ArraysEqual((sourceValue as Array), (targetValue as Array));

                    if (sourceValue is IDictionary &&
                        sourceValue.GetType().IsGenericType)
                        return DictionariesEqual((IDictionary)sourceValue, (IDictionary)targetValue);

                    if (sourceValue is ICollection &&
                        sourceValue.GetType().IsGenericType)
                        return CollectionsEqual((ICollection)sourceValue, (ICollection)targetValue);
                     
                    return (bool)sourceValue?.Equals(targetValue);
                },
                ApplySourceValueToTarget =
                    (source, target) =>
                    {
                        var value = sourceValueSelector(source);
                        if (targetSelector.Body is not MemberExpression memberSelectorExpression) return;
                        var property = memberSelectorExpression.Member as PropertyInfo;
                        if (property != null) property.SetValue(target, value, null);
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
            if (newTargets == null) throw new ArgumentNullException(nameof(newTargets));

            foreach (var sourceItem in sources)
            {
                TTarget target;

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

                foreach (var o in _operators.Where(o => !o.IsEqual(sourceItem, target)))
                {
                    if (result.Action == ActionEnum.NotModified)
                        result.Action = ActionEnum.Update;

                    o.ApplySourceValueToTarget(sourceItem, target);
                }

                patchResults.Add(result);
            }

            patchResults.AddRange(unprocessedTargets.Select(targetItem => new PatchOperation<TTarget> { Action = ActionEnum.Delete, Entity = targetItem }));

            return patchResults;
        }
    }
}