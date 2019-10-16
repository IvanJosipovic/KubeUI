using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorTable
{
    public interface IDataProvider<TableItem>
    {
        Task Update();

        Task SortBy(IColumn<TableItem> column);
        
        IColumn<TableItem> SortColumn { get; }

        bool SortDescending { get; }

        IEnumerable<TableItem> GetData();

        event Action OnChange;

        long PageSize { get; }

        long PageNumber { get; }

        long TotalCount { get; }

        Task FirstPage();
        Task NextPage();
        Task PreviousPage();
        Task LastPage();

        bool IsEditMode { get; }

        void ToggleEditMode();
    }
}