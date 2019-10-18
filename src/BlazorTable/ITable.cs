using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorTable
{
    interface ITable<T>
    {
        void AddColumn(IColumn<T> column);
        
        void RemoveColumn(IColumn<T> column);
    }
}
