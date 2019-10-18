using Microsoft.AspNetCore.Components;
using System;
using System.Linq.Expressions;

namespace BlazorTable
{
    public interface IColumn<TableItem>
    {
        string Title { get; set; }

        string Width { get; set; }

        bool Editable { get; set; }

        bool Sortable { get; set; }

        bool Filterable { get; set; }

        bool FilterOpen { get; set; }

        string GetPropertyName();

        void ToggleFilter();

        Expression<Func<TableItem, object>> Property { get; set; }

        RenderFragment<TableItem> EditorTemplate { get; set; }

        RenderFragment<TableItem> Template { get; set; }
    }
}
