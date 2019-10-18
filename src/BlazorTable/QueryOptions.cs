namespace BlazorTable
{
    public class QueryOptions : IQueryOptions
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool SortDescending { get; set; }

        public string SortColumn { get; set; }
    }
}
