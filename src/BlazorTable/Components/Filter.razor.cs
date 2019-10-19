using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorTable
{
    public partial class Filter<TableItem>
    {
        [CascadingParameter(Name = "Table")] public ITable<TableItem> Table { get; set; }

        [Parameter] public IColumn<TableItem> Column { get; set; }

        [Inject] public ILogger<Filter<TableItem>> Logger { get; set; }

        private Type MemberType;

        private StringFilters stringFilters { get; set; }

        private string filterText { get; set; }

        protected override void OnInitialized()
        {
            MemberType = Column.GetMemberUnderlyingType(Column.GetPropertyMemberInfo());
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrEmpty(filterText))
            {
                Logger.LogInformation("Filter Text is Null!");
                return;
            }

            if (Table == null)
            {
                Logger.LogInformation("Table is Null!");
                return;
            }

            Column.ToggleFilter();

            switch (stringFilters)
            {
                case StringFilters.Contains:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.Contains));
                    break;
                case StringFilters.Does_not_contain:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.Contains));
                    break;
                case StringFilters.Starts_with:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.StartsWith));
                    break;
                case StringFilters.Ends_with:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.EndsWith));
                    break;
                case StringFilters.Is_equal_to:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.Equals));
                    break;
                case StringFilters.Is_not_equal_to:
                    break;
                case StringFilters.Is_null:
                    break;
                case StringFilters.Is_not_null:
                    break;
                case StringFilters.Is_empty:
                    Column.Filter = AddFilterToStringProperty(Column.Property, filterText, nameof(string.IsNullOrEmpty));
                    break;
                case StringFilters.Is_not_empty:
                    break;
                default:
                    break;
            }

            Table.Update();
        }

        public static Expression<Func<T, bool>> AddFilterToStringProperty<T>(
        Expression<Func<T, object>> expression, string filter, string Method)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.Call(
                    expression.Body,
                    Method,
                    null,
                    Expression.Constant(filter)),
                expression.Parameters);
        }
    }
}
