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
            MemberType = Column.Property.GetPropertyMemberInfo().GetMemberUnderlyingType();
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
                    Column.Filter = Utillities.CallMethod(Column.Property, filterText, nameof(string.Contains));
                    break;
                case StringFilters.Does_not_contain:
                    Column.Filter = Utillities.Not(Utillities.CallMethod(Column.Property, filterText, nameof(string.Contains)));
                    break;
                case StringFilters.Starts_with:
                    Column.Filter = Utillities.CallMethod(Column.Property, filterText, nameof(string.StartsWith));
                    break;
                case StringFilters.Ends_with:
                    Column.Filter = Utillities.CallMethod(Column.Property, filterText, nameof(string.EndsWith));
                    break;
                case StringFilters.Is_equal_to:
                    Column.Filter = Utillities.CallMethod(Column.Property, filterText, nameof(string.Equals));
                    break;
                case StringFilters.Is_not_equal_to:
                    Column.Filter = Utillities.Not(Utillities.CallMethod(Column.Property, filterText, nameof(string.Equals)));
                    break;
                case StringFilters.Is_null_or_empty:
                    Column.Filter = Utillities.CallMethod(Column.Property, filterText, nameof(string.IsNullOrEmpty));
                    break;
                case StringFilters.Is_not_null_or_empty:
                    Column.Filter = Utillities.Not(Utillities.CallMethod(Column.Property, filterText, nameof(string.IsNullOrEmpty)));
                    break;
                default:
                    throw new ArgumentException(stringFilters + " is not defined!");
            }

            Table.Update();
        }
    }
}
