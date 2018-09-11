using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Eurofurence.App.Domain.Model
{
    public class FilterOptions<TEntity>
    {
        public int? MaxRecordCount { get; set; }

        public class SortField<TEntitySort>
        {
            public SortOrderEnum Order { get; set; }
            public Expression<Func<TEntitySort, object>> Field { get; set; }
        }

        public enum SortOrderEnum { Ascending, Descending }

        private readonly List<SortField<TEntity>> _sortFields;
        public IEnumerable<SortField<TEntity>> SortFields { get { return _sortFields; } }

        public FilterOptions()
        {
            _sortFields = new List<SortField<TEntity>>();
        }

        public FilterOptions<TEntity> SortAscending(Expression<Func<TEntity, object>> field)
        {
            _sortFields.Add(new SortField<TEntity>() { Order = SortOrderEnum.Ascending, Field = field });
            return this;
        }

        public FilterOptions<TEntity> SortDescending(Expression<Func<TEntity, object>> field)
        {
            _sortFields.Add(new SortField<TEntity>() { Order = SortOrderEnum.Descending, Field = field });
            return this;
        }

        public FilterOptions<TEntity> Take(int? maxRecordCount)
        {
            MaxRecordCount = maxRecordCount;
            return this;
        }
    }
}
