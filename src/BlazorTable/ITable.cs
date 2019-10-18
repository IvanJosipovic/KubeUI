using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorTable
{
    public interface ITable<TableItem>
    {
        void Update();

        List<IColumn<TableItem>> Columns { get; }

        void SortBy(IColumn<TableItem> column);
        
        IColumn<TableItem> SortColumn { get; }

        bool SortDescending { get; }

        long PageSize { get; }

        long PageNumber { get; }

        long TotalCount { get; }

        bool IsEditMode { get; }

        void FirstPage();

        void NextPage();

        void PreviousPage();

        void LastPage();

        void ToggleEditMode();

        void AddColumn(IColumn<TableItem> column);

        void RemoveColumn(IColumn<TableItem> column);
    }
}