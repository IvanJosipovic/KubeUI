using System.Collections.Generic;

namespace BlazorTable
{
    public class ResultObject<T>
    {
        public IEnumerable<T> Items { get; set; }

        public long TotalCount { get; set; }
    }
}
