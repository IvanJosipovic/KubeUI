using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorTable.Classes
{
    public abstract class DataProviderBase<TableItem> : IDataProvider<TableItem>
    {
        public IColumn<TableItem> SortColumn { get; set; }

        public bool SortDescending { get; set; }

        public long PageSize { get; set; } = 30;

        public long PageNumber { get; set; } = 0;

        public long TotalCount { get; set; }

        public bool IsEditMode { get; set; }

        public event Action OnChange;

        public abstract IEnumerable<TableItem> GetData();

        public async Task FirstPage()
        {
            PageNumber = 0;
            await Update();
        }
       
        public Task LastPage()
        {
            throw new NotImplementedException();
        }

        public Task NextPage()
        {
            throw new NotImplementedException();
        }

        public Task PreviousPage()
        {
            throw new NotImplementedException();
        }

        public Task SortBy(IColumn<TableItem> column)
        {
            throw new NotImplementedException();
        }

        public void ToggleEditMode()
        {
            throw new NotImplementedException();
        }

        public Task Update()
        {
            throw new NotImplementedException();
        }
    }
}
